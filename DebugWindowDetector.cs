using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kuaijiejian
{
    /// <summary>
    /// Windows 窗口检测调试工具
    /// 用于排查为什么无法检测到属性面板
    /// </summary>
    public static class DebugWindowDetector
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        
        /// <summary>
        /// 列出所有 Photoshop 窗口
        /// </summary>
        public static string DetectAllPhotoshopWindows()
        {
            var result = new StringBuilder();
            result.AppendLine("=== Photoshop 窗口检测报告 ===\n");
            
            try
            {
                // 查找 Photoshop 进程
                var psProcesses = Process.GetProcessesByName("Photoshop");
                if (psProcesses.Length == 0)
                {
                    result.AppendLine("❌ 未找到 Photoshop 进程");
                    return result.ToString();
                }
                
                var psProcess = psProcesses[0];
                result.AppendLine($"✅ 找到 Photoshop 进程");
                result.AppendLine($"   进程 ID: {psProcess.Id}");
                result.AppendLine($"   主窗口句柄: {psProcess.MainWindowHandle}");
                result.AppendLine($"   主窗口标题: {psProcess.MainWindowTitle}");
                result.AppendLine();
                
                // 枚举所有窗口，找到属于 Photoshop 的
                result.AppendLine("📋 所有属于 Photoshop 的窗口：");
                result.AppendLine("─────────────────────────");
                
                int windowCount = 0;
                var windows = new List<WindowInfo>();
                
                EnumWindows((hWnd, lParam) =>
                {
                    GetWindowThreadProcessId(hWnd, out uint processId);
                    
                    if (processId == psProcess.Id && IsWindowVisible(hWnd))
                    {
                        var titleSb = new StringBuilder(256);
                        var classSb = new StringBuilder(256);
                        GetWindowText(hWnd, titleSb, 256);
                        GetClassName(hWnd, classSb, 256);
                        
                        string title = titleSb.ToString();
                        string className = classSb.ToString();
                        
                        if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(className))
                        {
                            windows.Add(new WindowInfo
                            {
                                Handle = hWnd,
                                Title = title,
                                ClassName = className
                            });
                            windowCount++;
                        }
                    }
                    
                    return true;
                }, IntPtr.Zero);
                
                // 显示所有窗口
                foreach (var win in windows)
                {
                    result.AppendLine($"\n窗口 #{windows.IndexOf(win) + 1}:");
                    result.AppendLine($"  标题: {(string.IsNullOrEmpty(win.Title) ? "(无标题)" : win.Title)}");
                    result.AppendLine($"  类名: {win.ClassName}");
                    result.AppendLine($"  句柄: {win.Handle}");
                    
                    // 检查是否可能是属性面板
                    if (win.Title.Contains("属性") || win.Title.Contains("Properties"))
                    {
                        result.AppendLine($"  ⭐ 可能是属性面板！");
                    }
                }
                
                result.AppendLine($"\n共找到 {windowCount} 个可见窗口");
                
                // 枚举主窗口的子窗口
                result.AppendLine("\n\n📋 主窗口的子窗口：");
                result.AppendLine("─────────────────────────");
                
                int childCount = 0;
                var childWindows = new List<WindowInfo>();
                
                EnumChildWindows(psProcess.MainWindowHandle, (hWnd, lParam) =>
                {
                    if (IsWindowVisible(hWnd))
                    {
                        var titleSb = new StringBuilder(256);
                        var classSb = new StringBuilder(256);
                        GetWindowText(hWnd, titleSb, 256);
                        GetClassName(hWnd, classSb, 256);
                        
                        string title = titleSb.ToString();
                        string className = classSb.ToString();
                        
                        childWindows.Add(new WindowInfo
                        {
                            Handle = hWnd,
                            Title = title,
                            ClassName = className
                        });
                        childCount++;
                    }
                    
                    return true;
                }, IntPtr.Zero);
                
                // 只显示有标题的子窗口
                var namedChildren = childWindows.Where(w => !string.IsNullOrEmpty(w.Title)).ToList();
                
                if (namedChildren.Count > 0)
                {
                    foreach (var child in namedChildren)
                    {
                        result.AppendLine($"\n子窗口 #{namedChildren.IndexOf(child) + 1}:");
                        result.AppendLine($"  标题: {child.Title}");
                        result.AppendLine($"  类名: {child.ClassName}");
                        result.AppendLine($"  句柄: {child.Handle}");
                        
                        if (child.Title.Contains("属性") || child.Title.Contains("Properties"))
                        {
                            result.AppendLine($"  ⭐ 可能是属性面板！");
                        }
                    }
                    result.AppendLine($"\n共找到 {namedChildren.Count} 个有标题的子窗口（总共 {childCount} 个子窗口）");
                }
                else
                {
                    result.AppendLine($"\n未找到有标题的子窗口（共 {childCount} 个子窗口）");
                }
                
            }
            catch (Exception ex)
            {
                result.AppendLine($"\n❌ 检测失败：{ex.Message}");
                result.AppendLine($"   堆栈：{ex.StackTrace}");
            }
            
            return result.ToString();
        }
        
        private class WindowInfo
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; } = "";
            public string ClassName { get; set; } = "";
        }
    }
}


