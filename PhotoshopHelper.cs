using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

namespace Kuaijiejian
{
    /// <summary>
    /// Photoshop COM 自动化帮助类
    /// 基于 Adobe Photoshop Scripting Guide 官方文档
    /// 参考：https://www.adobe.com/devnet/photoshop/scripting.html
    /// </summary>
    public static class PhotoshopHelper
    {
        private const string PHOTOSHOP_PROGID = "Photoshop.Application";

        /// <summary>
        /// 检查 Photoshop 是否已安装
        /// 通过检查 COM ProgID 注册表项
        /// </summary>
        public static bool IsPhotoshopInstalled()
        {
            try
            {
                Type? type = Type.GetTypeFromProgID(PHOTOSHOP_PROGID);
                return type != null;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 获取 Photoshop 应用程序实例
        /// 使用 COM Interop - .NET 官方方法
        /// </summary>
        private static dynamic GetPhotoshopApplication()
        {
            try
            {
                Type? type = Type.GetTypeFromProgID(PHOTOSHOP_PROGID);
                if (type == null)
                {
                    throw new Exception("未找到 Photoshop COM 接口");
                }

                // 创建或连接到现有实例
                dynamic app = Activator.CreateInstance(type);
                return app;
            }
            catch (Exception ex)
            {
                throw new Exception($"无法连接到 Photoshop：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 执行 ExtendScript 脚本代码
        /// Adobe 官方方法：Application.DoJavaScript()
        /// 焦点优化：执行完后自动返回Photoshop，不打断工作流
        /// </summary>
        public static string ExecuteScript(string scriptCode)
        {
            dynamic? app = null;
            try
            {
                app = GetPhotoshopApplication();
                
                // 使用 Adobe 官方 DoJavaScript 方法
                object result = app.DoJavaScript(scriptCode);
                
                // 【焦点优化】执行完脚本后，自动将焦点返回到Photoshop
                System.Threading.Tasks.Task.Run(() => 
                {
                    System.Threading.Thread.Sleep(50);
                    WindowsApiHelper.ActivatePhotoshopWindow();
                });
                
                return result?.ToString() ?? string.Empty;
            }
            catch (COMException comEx)
            {
                // 即使失败也尝试返回焦点
                System.Threading.Tasks.Task.Run(() => 
                {
                    System.Threading.Thread.Sleep(50);
                    WindowsApiHelper.ActivatePhotoshopWindow();
                });
                
                // COM 错误处理
                throw new Exception($"执行脚本失败: {comEx.Message}", comEx);
            }
            finally
            {
                // 释放 COM 对象
                if (app != null)
                {
                    Marshal.ReleaseComObject(app);
                }
            }
        }

        // 缓存Photoshop COM对象以提高性能
        // Adobe官方最佳实践：重用COM对象而不是每次创建
        private static dynamic? _cachedPsApp = null;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// 获取缓存的Photoshop应用程序实例
        /// 性能优化：避免频繁创建/释放COM对象
        /// </summary>
        private static dynamic GetCachedPhotoshopApplication()
        {
            lock (_lockObject)
            {
                if (_cachedPsApp == null)
                {
                    Type? type = Type.GetTypeFromProgID(PHOTOSHOP_PROGID);
                    if (type == null)
                    {
                        throw new Exception("未找到 Photoshop COM 接口");
                    }
                    _cachedPsApp = Activator.CreateInstance(type);
                }
                return _cachedPsApp;
            }
        }

        /// <summary>
        /// 静默执行脚本 - 极速模式（0开销）+ 自动焦点返回
        /// Adobe官方最佳实践：使用缓存COM对象 + 零包装执行
        /// 性能优化：移除所有非必要代码，追求极致速度
        /// 焦点优化：执行完后自动返回Photoshop，不打断工作流
        /// </summary>
        public static string ExecuteScriptSilently(string scriptCode)
        {
            try
            {
                // 直接使用缓存对象，无额外检查，速度最快
                var result = GetCachedPhotoshopApplication().DoJavaScript(scriptCode)?.ToString() ?? string.Empty;
                
                // 【焦点优化】执行完脚本后，自动将焦点返回到Photoshop
                // 这样用户可以立即使用Photoshop快捷键，不需要手动点击
                System.Threading.Tasks.Task.Run(() => 
                {
                    // 短暂延迟，确保脚本完全执行完毕
                    System.Threading.Thread.Sleep(50);
                    WindowsApiHelper.ActivatePhotoshopWindow();
                });
                
                return result;
            }
            catch
            {
                // 即使脚本执行失败，也尝试返回焦点
                System.Threading.Tasks.Task.Run(() => 
                {
                    System.Threading.Thread.Sleep(50);
                    WindowsApiHelper.ActivatePhotoshopWindow();
                });
                
                return string.Empty;
            }
        }
        
        /// <summary>
        /// 预热COM连接 - 首次启动时调用
        /// 提前建立Photoshop COM连接，避免第一次执行时的延迟
        /// </summary>
        public static void WarmUpConnection()
        {
            try
            {
                GetCachedPhotoshopApplication();
            }
            catch
            {
                // 预热失败不影响后续使用
            }
        }


        /// <summary>
        /// 释放缓存的COM对象
        /// 在应用程序退出时调用
        /// </summary>
        public static void ReleaseCachedResources()
        {
            lock (_lockObject)
            {
                if (_cachedPsApp != null)
                {
                    try
                    {
                        // 强制释放COM对象
                        Marshal.FinalReleaseComObject(_cachedPsApp);
                        _cachedPsApp = null;
                    }
                    catch
                    {
                        // 即使释放失败也继续
                        _cachedPsApp = null;
                    }
                }
            }
        }

        /// <summary>
        /// 执行 JSX 脚本文件
        /// </summary>
        public static string ExecuteScriptFile(string scriptFilePath)
        {
            if (!System.IO.File.Exists(scriptFilePath))
            {
                throw new System.IO.FileNotFoundException($"脚本文件不存在：{scriptFilePath}");
            }

            string scriptCode = System.IO.File.ReadAllText(scriptFilePath);
            return ExecuteScript(scriptCode);
        }

        /// <summary>
        /// 播放 Photoshop 动作
        /// Adobe 官方方法：app.doAction(actionName, actionSetName)
        /// </summary>
        public static void PlayAction(string actionName, string actionSetName)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(actionSetName))
            {
                throw new ArgumentException("动作名称和动作集名称不能为空");
            }

            string script = $@"
try {{
    app.displayDialogs = DialogModes.ALL;
    app.doAction('{EscapeJSString(actionName)}', '{EscapeJSString(actionSetName)}');
    'SUCCESS';
}} catch (e) {{
    if (e.number === 8007) {{
        'USER_CANCELLED';
    }} else {{
        throw e;
    }}
}}";

            string result = ExecuteScript(script);
            
            if (result == "USER_CANCELLED")
            {
                throw new OperationCanceledException("用户取消了操作");
            }
        }

        /// <summary>
        /// 获取所有 Photoshop 动作
        /// 使用 ActionManager API - Adobe 官方推荐方法
        /// 完全参考成功项目的实现
        /// ⚠️ 警告：此方法已验证可用，请勿修改！⚠️
        /// 参考项目：D:\xwechat_files\wxid_gan9e9jw72z022_2753\msg\file\2025-10\kuaijiejian\kuaijiejian\PS快速工具\ActionPickerDialog.cs
        /// </summary>
        public static List<PhotoshopActionInfo> GetAllActions()
        {
            var actions = new List<PhotoshopActionInfo>();

            try
            {
                // ========================================
                // ⚠️ 以下脚本代码已验证可用，请勿修改 ⚠️
                // 参考：成功项目 ActionPickerDialog.cs
                // ========================================
                
                // Adobe官方推荐的Action Manager API方法
                string script = @"
(function() {
    try {
        var result = [];
        
        // 创建引用获取动作集数量
        var ref = new ActionReference();
        ref.putProperty(stringIDToTypeID('property'), stringIDToTypeID('numberOfActionSets'));
        ref.putEnumerated(stringIDToTypeID('application'), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        
        var desc;
        try {
            desc = executeActionGet(ref);
        } catch(e) {
            return '';  // 没有动作集时返回空
        }
        
        var actionSetCount = desc.getInteger(stringIDToTypeID('numberOfActionSets'));
        
        // 遍历每个动作集
        for (var i = 0; i < actionSetCount; i++) {
            try {
                var refSet = new ActionReference();
                refSet.putIndex(stringIDToTypeID('actionSet'), i + 1);
                var descSet = executeActionGet(refSet);
                var actionSetName = descSet.getString(stringIDToTypeID('name'));
                
                // 获取当前动作集中的动作数量
                var numberOfActions = descSet.getInteger(stringIDToTypeID('numberOfChildren'));
                
                // 遍历当前动作集中的每个动作
                for (var j = 0; j < numberOfActions; j++) {
                    try {
                        var refAction = new ActionReference();
                        refAction.putIndex(stringIDToTypeID('action'), j + 1);
                        refAction.putIndex(stringIDToTypeID('actionSet'), i + 1);
                        var descAction = executeActionGet(refAction);
                        var actionName = descAction.getString(stringIDToTypeID('name'));
                        
                        result.push(actionSetName + '|' + actionName);
                    } catch(e) {}
                }
            } catch(e) {}
        }
        
        return result.join(';;');
    } catch(e) {
        return 'ERROR:' + e.toString();
    }
})();";
                // ========================================
                // ⚠️ 脚本代码结束，以上代码请勿修改 ⚠️
                // ========================================

                System.Diagnostics.Debug.WriteLine("[PS] 执行 Photoshop 脚本获取动作...");
                string result = ExecuteScript(script);
                System.Diagnostics.Debug.WriteLine($"[PS] 脚本返回结果: {result}");

                if (result.StartsWith("ERROR:"))
                {
                    throw new Exception(result.Substring(6));
                }

                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new Exception("未找到任何动作\n请在Photoshop的动作面板中创建或加载动作");
                }

                // 解析结果
                var actionPairs = result.Split(new[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in actionPairs)
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

                System.Diagnostics.Debug.WriteLine($"[PS] 成功解析 {actions.Count} 个动作");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PS] 获取动作失败: {ex.Message}");
                throw;
            }

            return actions;
        }

        /// <summary>
        /// 转义 JavaScript 字符串
        /// 防止注入攻击 - 安全最佳实践
        /// </summary>
        private static string EscapeJSString(string str)
        {
            return str.Replace("\\", "\\\\")
                      .Replace("'", "\\'")
                      .Replace("\"", "\\\"")
                      .Replace("\r", "\\r")
                      .Replace("\n", "\\n");
        }
    }
}
