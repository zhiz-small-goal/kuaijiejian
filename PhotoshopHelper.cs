using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
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

        private static readonly object _comLock = new();
        private static dynamic? _cachedPsApp = null;
        private static string? _cachedProgId = null;
        private static Type? _cachedComType = null;

        /// <summary>
        /// 最近一次 COM 失败信息（用于诊断；不保证线程安全的强一致性）
        /// </summary>
        public static string? LastError { get; private set; }

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
            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, view);
                    foreach (var name in baseKey.GetSubKeyNames())
                    {
                        if (!name.StartsWith($"{PHOTOSHOP_BASE_PROGID}.", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var t = Type.GetTypeFromProgID(name, throwOnError: false);
                        if (t != null)
                        {
                            comType = t;
                            progId = name;
                            return true;
                        }
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

                // 创建 COM 实例；当目标版本已运行时，多数情况下会回到该实例（取决于 COM 服务器实现）
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

                    bool needRecreate =
                        _cachedPsApp == null ||
                        _cachedComType == null ||
                        _cachedProgId == null ||
                        !string.Equals(_cachedProgId, progId, StringComparison.OrdinalIgnoreCase);

                    if (!needRecreate)
                        return _cachedPsApp;

                    // 释放旧对象
                    InvalidateCache_NoLock();

                    // 创建新对象并缓存
                    _cachedPsApp = Activator.CreateInstance(comType);
                    _cachedComType = comType;
                    _cachedProgId = progId;

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
            }
        }

        /// <summary>
        /// 执行 Photoshop 脚本（带弹窗/错误抛出）
        /// </summary>
        public static string ExecuteScript(string scriptCode)
        {
            if (string.IsNullOrWhiteSpace(scriptCode))
                return string.Empty;

            dynamic? app = null;
            try
            {
                app = GetPhotoshopApplication();
                if (app == null)
                    throw new Exception(LastError ?? "无法获取 Photoshop COM 对象。");

                string result = app.DoJavaScript(scriptCode);
                LastError = null;
                return result ?? string.Empty;
            }
            finally
            {
                if (app != null && Marshal.IsComObject(app))
                {
                    try { Marshal.FinalReleaseComObject(app); } catch { }
                }
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
                var app = GetCachedPhotoshopApplication();
                if (app == null)
                    return string.Empty;

                try
                {
                    string result = app.DoJavaScript(scriptCode);
                    LastError = null;

                    // 执行完脚本后，尽量把焦点还给 Photoshop（避免打断工作流）
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        Thread.Sleep(50);
                        WindowsApiHelper.ActivatePhotoshopWindow();
                    });

                    return result ?? string.Empty;
                }
                catch (Exception ex1)
                {
                    // 常见场景：Photoshop 重启后旧的 COM 代理失效 / RPC 断开
                    LastError = $"DoJavaScript 失败（将重试一次）：{ex1.Message}";
                    InvalidateCache();

                    var app2 = GetCachedPhotoshopApplication();
                    if (app2 == null)
                        return string.Empty;

                    try
                    {
                        string result2 = app2.DoJavaScript(scriptCode);
                        LastError = null;

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            Thread.Sleep(50);
                            WindowsApiHelper.ActivatePhotoshopWindow();
                        });

                        return result2 ?? string.Empty;
                    }
                    catch (Exception ex2)
                    {
                        LastError = $"DoJavaScript 重试仍失败：{ex2.Message}";

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            Thread.Sleep(50);
                            WindowsApiHelper.ActivatePhotoshopWindow();
                        });

                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = $"静默执行异常：{ex.Message}";

                System.Threading.Tasks.Task.Run(() =>
                {
                    Thread.Sleep(50);
                    WindowsApiHelper.ActivatePhotoshopWindow();
                });

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
                    try { GetCachedPhotoshopApplication(); } catch { }
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
                dynamic? app = GetCachedPhotoshopApplication();
                if (app == null)
                    return actions;

                // 获取动作集数量
                int actionSetCount = app.ActionSets.Count;

                for (int i = 1; i <= actionSetCount; i++)
                {
                    dynamic actionSet = app.ActionSets[i];
                    string setName = actionSet.Name;

                    // 获取动作数量
                    int actionCount = actionSet.Actions.Count;

                    for (int j = 1; j <= actionCount; j++)
                    {
                        dynamic action = actionSet.Actions[j];
                        string actionName = action.Name;

                        actions.Add(new PhotoshopActionInfo
                        {
                            ActionSetName = setName,
                            ActionName = actionName
                        });
                    }
                }
            }
            catch
            {
                // 保持静默，调用方决定是否提示
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
try {{
    app.displayDialogs = DialogModes.ALL;
    app.doAction('{EscapeJSString(actionName)}', '{EscapeJSString(actionSetName)}');
}} catch(e) {{}}
";
                ExecuteScriptSilently(script);
                return true;
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
            try
            {
                dynamic? app = GetCachedPhotoshopApplication();
                if (app == null)
                    return string.Empty;

                if (app.Documents.Count > 0)
                    return app.ActiveDocument.Name;

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 检查是否有打开的文档
        /// </summary>
        public static bool HasOpenDocument()
        {
            try
            {
                dynamic? app = GetCachedPhotoshopApplication();
                if (app == null)
                    return false;

                return app.Documents.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取当前选中的图层名称
        /// </summary>
        public static string GetActiveLayerName()
        {
            try
            {
                dynamic? app = GetCachedPhotoshopApplication();
                if (app == null)
                    return string.Empty;

                if (app.Documents.Count > 0)
                    return app.ActiveDocument.ActiveLayer.Name;

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取 Photoshop 版本信息
        /// </summary>
        public static string GetPhotoshopVersion()
        {
            try
            {
                dynamic? app = GetCachedPhotoshopApplication();
                if (app == null)
                    return string.Empty;

                return app.Version;
            }
            catch
            {
                return string.Empty;
            }
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
