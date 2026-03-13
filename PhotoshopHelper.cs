using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Kuaijiejian
{
    /// <summary>
    /// Photoshop COM 自动化帮助类
    /// - 通过 COM ProgID（版本无关 / 版本相关）创建 Photoshop.Application
    /// - 支持多版本并存时优先绑定“正在运行的 Photoshop”版本
    /// 
    /// 说明：
    /// 1) Windows 下 Photoshop 支持通过 COM 自动化进行脚本控制（DoJavaScript 等）。
    /// 2) COM 的“版本无关 ProgID”可能通过 CurVer 指向“最新安装版本”，导致旧版本（例如 2021）已启动但无法被控制。
    /// </summary>
    public static class PhotoshopHelper
    {
        private const string PHOTOSHOP_BASE_PROGID = "Photoshop.Application";
        private const int TYPE_E_CANTLOADLIBRARY = unchecked((int)0x80029C4A);

        private static readonly object _comLock = new();
        private static readonly object _scriptHostLock = new();
        private static dynamic? _cachedPsApp = null;
        private static string? _cachedProgId = null;
        private static Type? _cachedComType = null;
        private static int? _cachedOwnerThreadId = null;
        private static ApartmentState _cachedOwnerApartment = ApartmentState.Unknown;
        private static string? _scriptHostRunnerPath = null;

        /// <summary>
        /// 最近一次 COM 失败信息（用于诊断；不保证线程安全的强一致性）
        /// </summary>
        public static string? LastError { get; private set; }

        private sealed class AutomationRegistrationInfo
        {
            public string ProgId { get; set; } = string.Empty;
            public string? Clsid { get; set; }
            public string? LocalServerPath { get; set; }
            public string? TypeLibId { get; set; }
            public string? RegisteredTypeLibPath { get; set; }
            public string? ExpectedTypeLibPath { get; set; }
        }

        
        /// <summary>
        /// 尝试连接到正在运行的 Photoshop COM 对象（Running Object Table）。
        /// 注意：在部分 .NET 目标框架下 Marshal.GetActiveObject 可能不可用，因此这里使用 P/Invoke 调用原生 COM API。
        /// 若不存在运行实例或未注册到 ROT，返回 null。
        /// </summary>
        private static dynamic? TryGetActiveObjectSafe(string progId)
        {
            try
            {
                return ComRot.TryGetActiveObjectFromProgId(progId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 通过原生 COM API 访问 ROT，避免依赖 Marshal.GetActiveObject（在部分目标框架中缺失该封装）
        /// </summary>
        private static class ComRot
        {
            [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
            private static extern int CLSIDFromProgIDEx(string lpszProgID, out Guid pclsid);

            [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
            private static extern int CLSIDFromProgID(string lpszProgID, out Guid pclsid);

            [DllImport("oleaut32.dll", PreserveSig = true)]
            private static extern int GetActiveObject(ref Guid rclsid, IntPtr pvReserved,
                [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

            public static object? TryGetActiveObjectFromProgId(string progId)
            {
                if (string.IsNullOrWhiteSpace(progId)) return null;

                Guid clsid;
                int hr = CLSIDFromProgIDEx(progId, out clsid);
                if (hr != 0)
                    hr = CLSIDFromProgID(progId, out clsid);
                if (hr != 0)
                    return null;

                object unk;
                hr = GetActiveObject(ref clsid, IntPtr.Zero, out unk);
                if (hr != 0)
                    return null;

                return unk;
            }
        }

        private static void ReturnFocusToPhotoshopAsync()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                Thread.Sleep(50);
                WindowsApiHelper.ActivatePhotoshopWindow();
            });
        }

        private static bool IsTypeLibraryFailure(Exception? ex)
        {
            while (ex != null)
            {
                if (ex is COMException comEx && comEx.HResult == TYPE_E_CANTLOADLIBRARY)
                    return true;

                ex = ex.InnerException;
            }

            return false;
        }

        private static string NormalizeScriptHostOutput(string output)
        {
            return output
                .Replace("\uFEFF", string.Empty)
                .TrimEnd('\r', '\n');
        }

        private static string? ExtractExecutablePath(string? commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return null;

            string trimmed = commandLine.Trim();
            if (trimmed.StartsWith("\"", StringComparison.Ordinal))
            {
                int endQuote = trimmed.IndexOf('"', 1);
                if (endQuote > 1)
                    return trimmed.Substring(1, endQuote - 1);
            }

            int exeIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex >= 0)
                return trimmed.Substring(0, exeIndex + 4).Trim();

            return trimmed;
        }

        private static string? TryReadTypeLibPath(RegistryKey baseKey, string typeLibId)
        {
            try
            {
                using var typeLibRoot = baseKey.OpenSubKey($@"TypeLib\{typeLibId}");
                if (typeLibRoot == null)
                    return null;

                var versionNames = typeLibRoot.GetSubKeyNames()
                    .OrderByDescending(v => v, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var version in versionNames)
                {
                    foreach (var platform in new[] { "win64", "win32" })
                    {
                        using var platformKey = typeLibRoot.OpenSubKey($@"{version}\0\{platform}");
                        var path = platformKey?.GetValue(null) as string;
                        if (!string.IsNullOrWhiteSpace(path))
                            return path.Trim();
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static bool TryGetAutomationRegistrationInfo(string progId, out AutomationRegistrationInfo? info)
        {
            info = null;
            if (string.IsNullOrWhiteSpace(progId))
                return false;

            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, view);
                    using var progKey = baseKey.OpenSubKey(progId);
                    if (progKey == null)
                        continue;

                    string? clsid = progKey.OpenSubKey("CLSID")?.GetValue(null) as string;
                    if (string.IsNullOrWhiteSpace(clsid))
                        continue;

                    using var clsidKey = baseKey.OpenSubKey($@"CLSID\{clsid}");
                    string? localServerCommandLine = clsidKey?.OpenSubKey("LocalServer32")?.GetValue(null) as string;
                    string? localServerPath = ExtractExecutablePath(localServerCommandLine);

                    string? typeLibId = clsidKey?.OpenSubKey("TypeLib")?.GetValue(null) as string;
                    string? registeredTypeLibPath = string.IsNullOrWhiteSpace(typeLibId)
                        ? null
                        : TryReadTypeLibPath(baseKey, typeLibId);

                    string? expectedTypeLibPath = null;
                    if (!string.IsNullOrWhiteSpace(localServerPath))
                    {
                        string? installDir = Path.GetDirectoryName(localServerPath);
                        if (!string.IsNullOrWhiteSpace(installDir))
                        {
                            expectedTypeLibPath = Path.Combine(
                                installDir,
                                "Required",
                                "Plug-ins",
                                "Extensions",
                                "ScriptingSupport.8li");
                        }
                    }

                    info = new AutomationRegistrationInfo
                    {
                        ProgId = progId,
                        Clsid = clsid,
                        LocalServerPath = localServerPath,
                        TypeLibId = typeLibId,
                        RegisteredTypeLibPath = registeredTypeLibPath,
                        ExpectedTypeLibPath = expectedTypeLibPath
                    };

                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        private static string? GetScriptHostFallbackReason(string? progId, Exception? ex = null)
        {
            if (IsTypeLibraryFailure(ex))
                return $"Photoshop 自动化类型库调用失败：{ex?.Message}";

            if (string.IsNullOrWhiteSpace(progId))
                return null;

            if (!TryGetAutomationRegistrationInfo(progId, out var info) || info == null)
                return null;

            if (string.IsNullOrWhiteSpace(info.RegisteredTypeLibPath))
                return null;

            if (File.Exists(info.RegisteredTypeLibPath))
                return null;

            if (!string.IsNullOrWhiteSpace(info.ExpectedTypeLibPath) && File.Exists(info.ExpectedTypeLibPath))
            {
                return $"检测到 Photoshop 自动化类型库注册指向缺失文件：{info.RegisteredTypeLibPath}；当前安装目录存在可用脚本支持文件：{info.ExpectedTypeLibPath}";
            }

            return $"检测到 Photoshop 自动化类型库注册指向缺失文件：{info.RegisteredTypeLibPath}";
        }

        private static string EnsureScriptHostRunnerPath()
        {
            lock (_scriptHostLock)
            {
                string bridgeDir = Path.Combine(Path.GetTempPath(), "Kuaijiejian", "PhotoshopBridge");
                Directory.CreateDirectory(bridgeDir);

                string runnerPath = Path.Combine(bridgeDir, "PhotoshopRunner.vbs");
                const string runnerCode = @"On Error Resume Next
Dim appRef
Dim result
Dim scriptPath
Dim scriptText
Dim stream
scriptPath = WScript.Arguments(0)

Set appRef = GetObject(, ""Photoshop.Application"")
If Err.Number <> 0 Then
    Err.Clear
    Set appRef = CreateObject(""Photoshop.Application"")
End If

If Err.Number <> 0 Then
    WScript.StdErr.WriteLine ""ERROR:CREATE:"" & Err.Description
    WScript.Quit 2
End If

Set stream = CreateObject(""ADODB.Stream"")
If Err.Number <> 0 Then
    WScript.StdErr.WriteLine ""ERROR:STREAM:"" & Err.Description
    WScript.Quit 4
End If

stream.Type = 2
stream.Charset = ""utf-8""
stream.Open
stream.LoadFromFile scriptPath
scriptText = stream.ReadText(-1)
stream.Close

result = appRef.DoJavaScript(scriptText)
If Err.Number <> 0 Then
    WScript.StdErr.WriteLine ""ERROR:EXEC:"" & Err.Description
    WScript.Quit 3
End If

If IsNull(result) Then
    WScript.Echo """"
Else
    WScript.Echo CStr(result)
End If
";

                File.WriteAllText(runnerPath, runnerCode, Encoding.ASCII);
                _scriptHostRunnerPath = runnerPath;
                return runnerPath;
            }
        }

        private static string ExecuteScriptViaScriptHost(string scriptCode, string reason)
        {
            string bridgeDir = Path.Combine(Path.GetTempPath(), "Kuaijiejian", "PhotoshopBridge");
            Directory.CreateDirectory(bridgeDir);

            string scriptPath = Path.Combine(bridgeDir, $"script-{Guid.NewGuid():N}.jsx");
            File.WriteAllText(scriptPath, scriptCode, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cscript.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                psi.ArgumentList.Add("//Nologo");
                psi.ArgumentList.Add(EnsureScriptHostRunnerPath());
                psi.ArgumentList.Add(scriptPath);

                using var process = Process.Start(psi);
                if (process == null)
                {
                    LastError = $"无法启动 cscript.exe，兼容模式不可用。原因：{reason}";
                    return string.Empty;
                }

                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string errorText = NormalizeScriptHostOutput(string.IsNullOrWhiteSpace(stderr) ? stdout : stderr);
                    LastError = string.IsNullOrWhiteSpace(errorText)
                        ? $"VBScript 兼容模式执行失败。原因：{reason}"
                        : $"VBScript 兼容模式执行失败：{errorText}";
                    return string.Empty;
                }

                LastError = null;
                return NormalizeScriptHostOutput(stdout);
            }
            catch (Exception ex)
            {
                LastError = $"VBScript 兼容模式异常：{ex.Message}";
                return string.Empty;
            }
            finally
            {
                try { File.Delete(scriptPath); } catch { }
            }
        }

#region ProgID/COM 解析（兼容多版本并存）

        /// <summary>
        /// 尝试获取正在运行的 Photoshop 进程对应的产品版本（主/次版本即可）。
        /// 若无运行实例或权限不足，返回 null。
        /// </summary>
        private static Version? TryGetRunningPhotoshopVersion()
        {
            try
            {
                // Photoshop 进程名通常为 "Photoshop"
                var processes = Process.GetProcessesByName("Photoshop");
                if (processes == null || processes.Length == 0)
                    return null;

                // 优先选择有主窗口的实例
                var ordered = processes
                    .OrderByDescending(p => p.MainWindowHandle != IntPtr.Zero)
                    .ToList();

                foreach (var p in ordered)
                {
                    try
                    {
                        var vi = p.MainModule?.FileVersionInfo;
                        if (vi == null)
                            continue;

                        // ProductMajorPart/MinorPart 更接近“产品版本”语义（例如 22.x）
                        int maj = vi.ProductMajorPart;
                        int min = vi.ProductMinorPart;

                        if (maj > 0)
                            return new Version(maj, Math.Max(0, min));
                    }
                    catch
                    {
                        // 进程模块信息读取可能因为权限/沙箱失败，继续尝试其他实例
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 尝试获取当前运行中的 Photoshop 可执行文件完整路径。
        /// </summary>
        private static string? TryGetRunningPhotoshopExecutablePath()
        {
            try
            {
                var processes = Process.GetProcessesByName("Photoshop");
                if (processes == null || processes.Length == 0)
                    return null;

                var ordered = processes
                    .OrderByDescending(p => p.MainWindowHandle != IntPtr.Zero)
                    .ToList();

                foreach (var p in ordered)
                {
                    try
                    {
                        var path = p.MainModule?.FileName;
                        if (!string.IsNullOrWhiteSpace(path))
                            return path;
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// 读取 HKCR\Photoshop.Application\CurVer 的默认值（若存在）
        /// CurVer 是 COM 的“版本无关 ProgID -> 当前版本 ProgID”指针。
        /// </summary>
        private static string? TryReadCurVerProgId()
        {
            try
            {
                // 先读 64-bit 视图（Photoshop 现代版本通常为 64-bit）
                string? cur = TryReadCurVerProgId(RegistryView.Registry64);
                if (!string.IsNullOrWhiteSpace(cur))
                    return cur;

                // 再读 32-bit 视图作为兜底
                return TryReadCurVerProgId(RegistryView.Registry32);
            }
            catch
            {
                return null;
            }
        }

        private static string? TryReadCurVerProgId(RegistryView view)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, view);
                using var curVerKey = baseKey.OpenSubKey($"{PHOTOSHOP_BASE_PROGID}\\CurVer");
                if (curVerKey == null)
                    return null;

                // (default) 值
                var value = curVerKey.GetValue(null) as string;
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 基于运行中的 Photoshop 版本构造若干“可能存在”的版本相关 ProgID。
        /// 经验上 Adobe 会注册形如 Photoshop.Application.150 / .220 等版本号（例如 15.0 -> 150）。
        /// 此处不把该映射当作事实：以“存在的注册项”为准。
        /// </summary>
        private static IEnumerable<string> BuildProgIdCandidatesForVersion(Version v)
        {
            var candidates = new List<string>();

            // 形态 1：Photoshop.Application.{major}
            candidates.Add($"{PHOTOSHOP_BASE_PROGID}.{v.Major}");

            // 形态 2：Photoshop.Application.{major*10+minor}（例如 22.0 -> 220）
            try
            {
                int v10 = checked(v.Major * 10 + v.Minor);
                candidates.Add($"{PHOTOSHOP_BASE_PROGID}.{v10}");
            }
            catch
            {
                // ignore overflow (very unlikely)
            }

            // 形态 3：Photoshop.Application.{major*10}（当 minor != 0 时给一个近似）
            try
            {
                int v10Major = checked(v.Major * 10);
                candidates.Add($"{PHOTOSHOP_BASE_PROGID}.{v10Major}");
            }
            catch
            {
            }

            // 形态 4：Photoshop.Application.{major}.{minor}（少数 COM 组件会这样）
            candidates.Add($"{PHOTOSHOP_BASE_PROGID}.{v.Major}.{v.Minor}");

            // 去重
            return candidates
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> EnumerateRegisteredProgIds()
        {
            var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                PHOTOSHOP_BASE_PROGID
            };

            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, view);
                    foreach (var name in baseKey.GetSubKeyNames())
                    {
                        if (name.Equals(PHOTOSHOP_BASE_PROGID, StringComparison.OrdinalIgnoreCase) ||
                            name.StartsWith($"{PHOTOSHOP_BASE_PROGID}.", StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add(name);
                        }
                    }
                }
                catch
                {
                }
            }

            return results
                .OrderBy(name =>
                {
                    if (name.Equals(PHOTOSHOP_BASE_PROGID, StringComparison.OrdinalIgnoreCase))
                        return 0;

                    int segmentCount = name.Split('.').Length;
                    return segmentCount == 3 ? 1 : 2;
                })
                .ThenBy(name => name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 尝试解析一个可用的 Photoshop COM Type 与对应 ProgID。
        /// preferRunningVersion=true：优先匹配“正在运行的 Photoshop 版本”，用于多版本并存。
        /// preferRunningVersion=false：用于“是否安装”检测（尽量返回任一可用 ProgID）。
        /// </summary>
        private static bool TryResolvePhotoshopComType(
            bool preferRunningVersion,
            out Type? comType,
            out string? progId)
        {
            comType = null;
            progId = null;

            // 1) 如果要求优先运行版本，则先用进程版本推导候选 ProgID
            if (preferRunningVersion)
            {
                var runningExePath = TryGetRunningPhotoshopExecutablePath();
                if (!string.IsNullOrWhiteSpace(runningExePath))
                {
                    foreach (var registeredProgId in EnumerateRegisteredProgIds())
                    {
                        if (!TryGetAutomationRegistrationInfo(registeredProgId, out var registration) ||
                            registration == null ||
                            string.IsNullOrWhiteSpace(registration.LocalServerPath))
                        {
                            continue;
                        }

                        if (!string.Equals(
                                registration.LocalServerPath,
                                runningExePath,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var matchedType = Type.GetTypeFromProgID(registeredProgId, throwOnError: false);
                        if (matchedType != null)
                        {
                            comType = matchedType;
                            progId = registeredProgId;
                            return true;
                        }
                    }
                }

                var runningVer = TryGetRunningPhotoshopVersion();
                if (runningVer != null)
                {
                    foreach (var candidate in BuildProgIdCandidatesForVersion(runningVer))
                    {
                        var t = Type.GetTypeFromProgID(candidate, throwOnError: false);
                        if (t != null)
                        {
                            comType = t;
                            progId = candidate;
                            return true;
                        }
                    }
                }
            }

            // 2) 版本无关 ProgID（可能存在，也可能因安装/卸载/权限导致缺失）
            {
                var t = Type.GetTypeFromProgID(PHOTOSHOP_BASE_PROGID, throwOnError: false);
                if (t != null)
                {
                    comType = t;
                    progId = PHOTOSHOP_BASE_PROGID;
                    return true;
                }
            }

            // 3) CurVer 指向的当前版本 ProgID（Windows COM 侧的标准机制）
            {
                var cur = TryReadCurVerProgId();
                if (!string.IsNullOrWhiteSpace(cur))
                {
                    var t = Type.GetTypeFromProgID(cur, throwOnError: false);
                    if (t != null)
                    {
                        comType = t;
                        progId = cur;
                        return true;
                    }
                }
            }

            // 4) 兜底：枚举注册表中所有 Photoshop.Application.*（仅在前面都失败时触发）
            //    这一步开销相对更高，因此放到最后。
            foreach (var name in EnumerateRegisteredProgIds())
            {
                try
                {
                    if (name.Equals(PHOTOSHOP_BASE_PROGID, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var t = Type.GetTypeFromProgID(name, throwOnError: false);
                    if (t != null)
                    {
                        comType = t;
                        progId = name;
                        return true;
                    }
                }
                catch
                {
                    // ignore
                }
            }

            return false;
        }

        #endregion

        /// <summary>
        /// 检查 Photoshop 是否已安装（至少存在一个可用的 COM ProgID）
        /// </summary>
        public static bool IsPhotoshopInstalled()
        {
            return TryResolvePhotoshopComType(
                preferRunningVersion: false,
                out _,
                out _);
        }

        /// <summary>
        /// 获取 Photoshop 应用程序对象（不缓存，每次新建/连接）
        /// </summary>
        public static dynamic? GetPhotoshopApplication()
        {
            try
            {
                if (!TryResolvePhotoshopComType(
                        preferRunningVersion: true,
                        out var comType,
                        out var progId)
                    || comType == null
                    || string.IsNullOrWhiteSpace(progId))
                {
                    LastError = "无法解析 Photoshop COM ProgID（注册表无对应项或权限不足）。";
                    return null;
                }

                var activeApp = TryGetActiveObjectSafe(progId);
                if (activeApp == null && !string.Equals(progId, PHOTOSHOP_BASE_PROGID, StringComparison.OrdinalIgnoreCase))
                    activeApp = TryGetActiveObjectSafe(PHOTOSHOP_BASE_PROGID);

                if (activeApp != null)
                {
                    LastError = null;
                    return activeApp;
                }

                var app = Activator.CreateInstance(comType);
                LastError = null;
                return app;
            }
            catch (Exception ex)
            {
                LastError = $"创建 Photoshop COM 实例失败：{ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// 获取缓存的 Photoshop 应用程序对象（会随“当前运行版本”动态切换）
        /// </summary>
        private static dynamic? GetCachedPhotoshopApplication()
        {
            lock (_comLock)
            {
                try
                {
                    if (!TryResolvePhotoshopComType(
                            preferRunningVersion: true,
                            out var comType,
                            out var progId)
                        || comType == null
                        || string.IsNullOrWhiteSpace(progId))
                    {
                        LastError = "无法解析 Photoshop COM ProgID（注册表无对应项或权限不足）。";
                        InvalidateCache_NoLock();
                        return null;
                    }

                    int currentThreadId = Environment.CurrentManagedThreadId;
                    ApartmentState currentApartment = ApartmentState.Unknown;
                    try { currentApartment = Thread.CurrentThread.GetApartmentState(); } catch { }

                    bool needRecreate =
                        _cachedPsApp == null ||
                        _cachedComType == null ||
                        _cachedProgId == null ||
                        _cachedOwnerThreadId != currentThreadId ||
                        !string.Equals(_cachedProgId, progId, StringComparison.OrdinalIgnoreCase);

                    if (!needRecreate)
                        return _cachedPsApp;

                    // 释放旧对象
                    InvalidateCache_NoLock();

                    // 创建新对象并缓存
                    // 优先连接已运行实例（ROT）；失败再创建新实例
                    _cachedPsApp = TryGetActiveObjectSafe(progId);
                    if (_cachedPsApp == null && !string.Equals(progId, PHOTOSHOP_BASE_PROGID, StringComparison.OrdinalIgnoreCase))
                        _cachedPsApp = TryGetActiveObjectSafe(PHOTOSHOP_BASE_PROGID);

                    if (_cachedPsApp == null)
                        _cachedPsApp = Activator.CreateInstance(comType);
                    _cachedComType = comType;
                    _cachedProgId = progId;
                    _cachedOwnerThreadId = currentThreadId;
                    _cachedOwnerApartment = currentApartment;

                    LastError = null;
                    return _cachedPsApp;
                }
                catch (Exception ex)
                {
                    LastError = $"获取缓存 Photoshop COM 实例失败：{ex.Message}";
                    InvalidateCache_NoLock();
                    return null;
                }
            }
        }

        /// <summary>
        /// 释放缓存的 COM 对象
        /// </summary>
        private static void InvalidateCache()
        {
            lock (_comLock)
            {
                InvalidateCache_NoLock();
            }
        }

        private static void InvalidateCache_NoLock()
        {
            try
            {
                if (_cachedPsApp != null && Marshal.IsComObject(_cachedPsApp))
                {
                    try { Marshal.FinalReleaseComObject(_cachedPsApp); } catch { }
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                _cachedPsApp = null;
                _cachedProgId = null;
                _cachedComType = null;
                _cachedOwnerThreadId = null;
                _cachedOwnerApartment = ApartmentState.Unknown;
            }
        }

        /// <summary>
        /// 执行 Photoshop 脚本（带弹窗/错误抛出）
        /// </summary>
        private static string ExecuteScriptViaFreshCom(string scriptCode)
        {
            dynamic? app = null;
            try
            {
                app = GetPhotoshopApplication();
                if (app == null)
                    throw new Exception(LastError ?? "无法获取 Photoshop COM 对象。");

                object? result = app.DoJavaScript(scriptCode);
                LastError = null;
                return result?.ToString() ?? string.Empty;
            }
            finally
            {
                if (app != null && Marshal.IsComObject(app))
                {
                    try { Marshal.FinalReleaseComObject(app); } catch { }
                }
            }
        }

        private static string ExecuteScriptViaCachedCom(string scriptCode)
        {
            var app = GetCachedPhotoshopApplication();
            if (app == null)
                throw new Exception(LastError ?? "无法获取 Photoshop COM 对象。");

            object? result = app.DoJavaScript(scriptCode);
            LastError = null;
            return result?.ToString() ?? string.Empty;
        }

        public static string ExecuteScript(string scriptCode)
        {
            if (string.IsNullOrWhiteSpace(scriptCode))
                return string.Empty;

            try
            {
                if (TryResolvePhotoshopComType(
                        preferRunningVersion: true,
                        out _,
                        out var progId))
                {
                    string? fallbackReason = GetScriptHostFallbackReason(progId);
                    if (!string.IsNullOrWhiteSpace(fallbackReason))
                    {
                        string fallbackResult = ExecuteScriptViaScriptHost(scriptCode, fallbackReason);
                        if (!string.IsNullOrWhiteSpace(LastError))
                            throw new Exception(LastError);

                        return fallbackResult;
                    }
                }

                return ExecuteScriptViaFreshCom(scriptCode);
            }
            catch (Exception ex) when (IsTypeLibraryFailure(ex))
            {
                string fallbackResult = ExecuteScriptViaScriptHost(
                    scriptCode,
                    GetScriptHostFallbackReason(_cachedProgId, ex) ?? $"Photoshop 自动化调用失败：{ex.Message}");

                if (!string.IsNullOrWhiteSpace(LastError))
                    throw new Exception(LastError);

                return fallbackResult;
            }
        }

        /// <summary>
        /// 静默执行 Photoshop 脚本（不弹框），但会做一次“缓存失效重试”，提高兼容性
        /// </summary>
        public static string ExecuteScriptSilently(string scriptCode)
        {
            if (string.IsNullOrWhiteSpace(scriptCode))
                return string.Empty;

            try
            {
                if (TryResolvePhotoshopComType(
                        preferRunningVersion: true,
                        out _,
                        out var progId))
                {
                    string? fallbackReason = GetScriptHostFallbackReason(progId);
                    if (!string.IsNullOrWhiteSpace(fallbackReason))
                    {
                        string fallbackResult = ExecuteScriptViaScriptHost(scriptCode, fallbackReason);
                        ReturnFocusToPhotoshopAsync();
                        return fallbackResult;
                    }
                }

                try
                {
                    string result = ExecuteScriptViaCachedCom(scriptCode);
                    ReturnFocusToPhotoshopAsync();
                    return result;
                }
                catch (Exception ex1)
                {
                    if (IsTypeLibraryFailure(ex1))
                    {
                        string fallbackResult = ExecuteScriptViaScriptHost(
                            scriptCode,
                            GetScriptHostFallbackReason(_cachedProgId, ex1) ?? $"Photoshop 自动化调用失败：{ex1.Message}");
                        ReturnFocusToPhotoshopAsync();
                        return fallbackResult;
                    }

                    // 常见场景：Photoshop 重启后旧的 COM 代理失效 / RPC 断开
                    LastError = $"DoJavaScript 失败（将重试一次）：{ex1.Message}";
                    InvalidateCache();

                    try
                    {
                        string result2 = ExecuteScriptViaCachedCom(scriptCode);
                        ReturnFocusToPhotoshopAsync();
                        return result2;
                    }
                    catch (Exception ex2)
                    {
                        if (IsTypeLibraryFailure(ex2))
                        {
                            string fallbackResult = ExecuteScriptViaScriptHost(
                                scriptCode,
                                GetScriptHostFallbackReason(_cachedProgId, ex2) ?? $"Photoshop 自动化调用失败：{ex2.Message}");
                            ReturnFocusToPhotoshopAsync();
                            return fallbackResult;
                        }

                        LastError = $"DoJavaScript 重试仍失败：{ex2.Message}";
                        ReturnFocusToPhotoshopAsync();
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = $"静默执行异常：{ex.Message}";
                ReturnFocusToPhotoshopAsync();
                return string.Empty;
            }
        }

        /// <summary>
        /// 预热 COM 连接（尽量在 STA 线程执行），减少首次执行脚本的冷启动开销
        /// </summary>
        public static void WarmUpConnection()
        {
            try
            {
                var t = new Thread(() =>
                {
                    dynamic? app = null;
                    try
                    {
                        if (TryResolvePhotoshopComType(
                                preferRunningVersion: true,
                                out _,
                                out var progId))
                        {
                            string? fallbackReason = GetScriptHostFallbackReason(progId);
                            if (!string.IsNullOrWhiteSpace(fallbackReason))
                            {
                                ExecuteScriptViaScriptHost("(function(){ return app.version; })();", fallbackReason);
                                return;
                            }
                        }

                        app = GetPhotoshopApplication();
                        if (app != null)
                        {
                            try { app.DoJavaScript("app.version"); } catch { }
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (app != null && Marshal.IsComObject(app))
                        {
                            try { Marshal.FinalReleaseComObject(app); } catch { }
                        }
                    }
                })
                { IsBackground = true };

                try { t.SetApartmentState(ApartmentState.STA); } catch { }
                t.Start();

                // 给一点时间预热；不阻塞过久
                t.Join(1500);
            }
            catch
            {
                // 预热失败不影响后续使用
            }
        }

        /// <summary>
        /// 释放缓存的 COM 对象（程序退出时调用）
        /// </summary>
        public static void ReleaseCachedResources()
        {
            InvalidateCache();
        }


        
        /// <summary>
        /// Escape 单引号/反斜杠等，避免拼接到 JSX 字符串时语法错误
        /// </summary>
        private static string EscapeJSString(string value)
        {
            if (value == null) return string.Empty;
            return value
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

/// <summary>
        /// 获取 Photoshop 中所有动作信息（Action Sets / Actions）
        /// </summary>
        public static List<PhotoshopActionInfo> GetAllActions()
        {
            var actions = new List<PhotoshopActionInfo>();

            try
            {
                // 说明：
                // 1) 直接访问 COM 对象的 ActionSets/Actions 集合在部分版本（例如 PS 2021）不可用或行为不一致。
                // 2) 使用 ExtendScript(Action Manager) 在 Photoshop 内部枚举动作，兼容性更好。
                //
                // 实现策略：
                // - 用 charID 常量（ASet/Actn/Nm  /NmbC）循环枚举动作集与子动作；
                // - 不依赖 “numberOfActionSets” 之类属性，避免版本差异；
                // - 返回 "set|action;;set|action" 的扁平字符串给 C# 解析。
                string script = @"
(function () {
    try {
        function cTID(s) { return app.charIDToTypeID(s); }

        var result = [];
        var i = 1;

        while (true) {
            if (i > 5000) break; // 防御：避免异常环境下死循环

            var refSet = new ActionReference();
            refSet.putIndex(cTID('ASet'), i);

            var descSet;
            try {
                descSet = executeActionGet(refSet);
            } catch (e) {
                break; // i 超出范围时会抛错，结束枚举
            }

            var setName = '';
            try {
                setName = descSet.getString(cTID('Nm  '));
            } catch (e) {
                setName = '';
            }

            var numberOfChildren = 0;
            try {
                numberOfChildren = descSet.getInteger(cTID('NmbC')); // numberOfChildren
            } catch (e) {
                numberOfChildren = 0;
            }

            for (var j = 1; j <= numberOfChildren; j++) {
                var refAction = new ActionReference();
                refAction.putIndex(cTID('Actn'), j);
                refAction.putIndex(cTID('ASet'), i);

                try {
                    var descAction = executeActionGet(refAction);
                    var actionName = descAction.getString(cTID('Nm  '));

                    if (setName && actionName) {
                        result.push(setName + '|' + actionName);
                    }
                } catch (e) { }
            }

            i++;
        }

        return result.join(';;');
    } catch (e) {
        return 'ERROR:' + e.toString();
    }
})();
";

                string result = ExecuteScriptSilently(script);

                if (!string.IsNullOrWhiteSpace(result) && result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                {
                    LastError = result;
                    return actions;
                }

                if (string.IsNullOrWhiteSpace(result))
                {
                    // 可能确实没有载入动作，也可能脚本被阻止/执行失败但未返回错误
                    return actions;
                }

                var pairs = result.Split(new[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var parts = pair.Split('|');
                    if (parts.Length == 2)
                    {
                        actions.Add(new PhotoshopActionInfo
                        {
                            ActionSetName = parts[0].Trim(),
                            ActionName = parts[1].Trim()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = $"获取动作列表失败：{ex.Message}";
            }

            return actions;
        }

        /// <summary>
        /// 执行 Photoshop 动作（app.DoAction）
        /// </summary>
        public static bool PlayAction(string actionName, string actionSetName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(actionName) || string.IsNullOrWhiteSpace(actionSetName))
                    return false;

                string script = $@"
((function () {{
    try {{
        app.displayDialogs = DialogModes.ALL;
        app.doAction('{EscapeJSString(actionName)}', '{EscapeJSString(actionSetName)}');
        return 'OK';
    }} catch(e) {{
        return 'ERROR:' + e.toString();
    }}
}})());
";
                string result = ExecuteScriptSilently(script);
                if (!string.IsNullOrWhiteSpace(result) &&
                    result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                {
                    LastError = $"执行动作失败：{result}";
                    return false;
                }

                return string.IsNullOrWhiteSpace(LastError);
            }
            catch (Exception ex)
            {
                LastError = $"执行动作失败：{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 获取当前活动文档名称
        /// </summary>
        public static string GetActiveDocumentName()
        {
            return ExecuteScriptSilently(@"
((function () {
    try {
        return app.documents.length > 0 ? app.activeDocument.name : '';
    } catch (e) {
        return '';
    }
})());
");
        }

        /// <summary>
        /// 检查是否有打开的文档
        /// </summary>
        public static bool HasOpenDocument()
        {
            string result = ExecuteScriptSilently(@"
((function () {
    try {
        return app.documents.length > 0 ? '1' : '0';
    } catch (e) {
        return '0';
    }
})());
");

            return string.Equals(result?.Trim(), "1", StringComparison.Ordinal);
        }

        /// <summary>
        /// 获取当前选中的图层名称
        /// </summary>
        public static string GetActiveLayerName()
        {
            return ExecuteScriptSilently(@"
((function () {
    try {
        return app.documents.length > 0 ? app.activeDocument.activeLayer.name : '';
    } catch (e) {
        return '';
    }
})());
");
        }

        /// <summary>
        /// 获取 Photoshop 版本信息
        /// </summary>
        public static string GetPhotoshopVersion()
        {
            return ExecuteScriptSilently("app.version");
        }

        /// <summary>
        /// 强制释放缓存的 COM 对象（例如在设置页面提供“重连”按钮时调用）
        /// </summary>
        public static void ResetComCache()
        {
            InvalidateCache();
        }
    }
}
