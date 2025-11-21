using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Kuaijiejian
{
    /// <summary>
    /// Windows API 辅助类
    /// 用于窗口焦点管理和高级窗口操作
    /// </summary>
    public static class WindowsApiHelper
    {
        #region Windows API 声明

        /// <summary>
        /// 设置窗口为前台窗口（激活并获得焦点）
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 根据窗口类名和窗口标题查找窗口
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        /// <summary>
        /// 枚举所有顶层窗口
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        /// <summary>
        /// 获取窗口进程ID
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// 检查窗口是否可见
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// 获取窗口标题文本长度
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// 获取窗口标题文本
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// 枚举窗口回调委托
        /// </summary>
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// 获取当前前台窗口句柄
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        #endregion

        #region 扩展窗口样式常量

        /// <summary>
        /// 扩展窗口样式：窗口不激活（不抢焦点）
        /// 当用户点击窗口时，窗口不会成为前台窗口
        /// </summary>
        public const int WS_EX_NOACTIVATE = 0x08000000;

        /// <summary>
        /// 扩展窗口样式：工具窗口
        /// 通常用于浮动工具面板
        /// </summary>
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        /// <summary>
        /// 获取窗口扩展样式
        /// </summary>
        public const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion

        #region 公共方法

        /// <summary>
        /// 激活Photoshop窗口（将焦点返回到Photoshop）
        /// 支持多种Photoshop版本的窗口标题
        /// </summary>
        /// <returns>是否成功激活</returns>
        public static bool ActivatePhotoshopWindow()
        {
            try
            {
                // 方法1：通过进程名查找Photoshop窗口
                IntPtr psWindow = FindPhotoshopWindowByProcess();
                if (psWindow != IntPtr.Zero)
                {
                    SetForegroundWindow(psWindow);
                    return true;
                }

                // 方法2：通过常见的窗口标题关键字查找
                string[] photoshopKeywords = { "Adobe Photoshop", "Photoshop" };
                foreach (string keyword in photoshopKeywords)
                {
                    psWindow = FindWindowByPartialTitle(keyword);
                    if (psWindow != IntPtr.Zero)
                    {
                        SetForegroundWindow(psWindow);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                // 静默失败，不影响脚本执行
                return false;
            }
        }

        /// <summary>
        /// 通过进程名查找Photoshop窗口句柄
        /// </summary>
        private static IntPtr FindPhotoshopWindowByProcess()
        {
            try
            {
                // 查找Photoshop进程
                Process[] processes = Process.GetProcessesByName("Photoshop");
                if (processes.Length == 0)
                {
                    return IntPtr.Zero;
                }

                // 获取第一个Photoshop进程的主窗口
                foreach (var process in processes)
                {
                    if (process.MainWindowHandle != IntPtr.Zero && IsWindowVisible(process.MainWindowHandle))
                    {
                        return process.MainWindowHandle;
                    }
                }

                return IntPtr.Zero;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 根据部分标题查找窗口
        /// </summary>
        private static IntPtr FindWindowByPartialTitle(string partialTitle)
        {
            IntPtr foundWindow = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0)
                    return true;

                System.Text.StringBuilder sb = new System.Text.StringBuilder(length + 1);
                GetWindowText(hWnd, sb, sb.Capacity);

                if (sb.ToString().Contains(partialTitle, StringComparison.OrdinalIgnoreCase))
                {
                    foundWindow = hWnd;
                    return false; // 停止枚举
                }

                return true; // 继续枚举
            }, IntPtr.Zero);

            return foundWindow;
        }

        /// <summary>
        /// 设置窗口为不激活样式（点击不抢焦点）
        /// 适用于工具面板类窗口
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        public static void SetWindowNoActivate(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle == IntPtr.Zero)
                    return;

                int exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
                SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);
            }
            catch
            {
                // 静默失败，不影响程序运行
            }
        }

        /// <summary>
        /// 判断指定窗口是否属于当前进程（包括主窗口和所有子窗口/对话框）
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <returns>是否属于当前进程</returns>
        public static bool IsCurrentProcessWindow(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle == IntPtr.Zero)
                    return false;

                // 获取窗口所属进程ID
                GetWindowThreadProcessId(windowHandle, out uint processId);
                
                // 获取当前进程ID
                int currentProcessId = Process.GetCurrentProcess().Id;
                
                // 比较进程ID
                return processId == currentProcessId;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断指定窗口是否是Photoshop窗口
        /// 支持多种Photoshop版本
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <returns>是否是Photoshop窗口</returns>
        public static bool IsPhotoshopWindow(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle == IntPtr.Zero)
                    return false;

                // 方法1：检查窗口所属进程名
                GetWindowThreadProcessId(windowHandle, out uint processId);
                try
                {
                    Process process = Process.GetProcessById((int)processId);
                    string processName = process.ProcessName.ToLower();
                    
                    // Photoshop的进程名通常是 "Photoshop"
                    if (processName.Contains("photoshop"))
                    {
                        return true;
                    }
                }
                catch
                {
                    // 进程已退出或无法访问，继续使用其他方法
                }

                // 方法2：检查窗口标题
                int length = GetWindowTextLength(windowHandle);
                if (length > 0)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(length + 1);
                    GetWindowText(windowHandle, sb, sb.Capacity);
                    string title = sb.ToString();

                    // 检查标题中是否包含 "Photoshop" 或 "Adobe Photoshop"
                    if (title.Contains("Photoshop", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}

