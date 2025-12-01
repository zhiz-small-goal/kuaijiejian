using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Kuaijiejian
{
    /// <summary>
    /// Windows API 帮助类，封装窗口枚举与前台窗口相关操作
    /// </summary>
    public static class WindowsApiHelper
    {
        #region Windows API

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        #endregion

        #region 扩展样式与窗口命令

        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int GWL_EXSTYLE = -20;

        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOWMINNOACTIVE = 7;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion

        #region 窗口操作

        /// <summary>
        /// 激活 Photoshop 窗口（将焦点返回到 Photoshop），支持多版本标题
        /// </summary>
        public static bool ActivatePhotoshopWindow()
        {
            try
            {
                IntPtr psWindow = FindPhotoshopWindowByProcess();
                if (psWindow != IntPtr.Zero)
                {
                    SetForegroundWindow(psWindow);
                    return true;
                }

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
                return false;
            }
        }

        /// <summary>
        /// 通过进程名查找 Photoshop 主窗口
        /// </summary>
        private static IntPtr FindPhotoshopWindowByProcess()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("Photoshop");
                if (processes.Length == 0)
                {
                    return IntPtr.Zero;
                }

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
        /// 按窗口标题关键字查找窗口
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

                StringBuilder sb = new StringBuilder(length + 1);
                GetWindowText(hWnd, sb, sb.Capacity);

                if (sb.ToString().Contains(partialTitle, StringComparison.OrdinalIgnoreCase))
                {
                    foundWindow = hWnd;
                    return false;
                }

                return true;
            }, IntPtr.Zero);

            return foundWindow;
        }

        /// <summary>
        /// 设置窗口为不激活样式（WS_EX_NOACTIVATE）
        /// </summary>
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
            }
        }

        /// <summary>
        /// 在不抢占焦点的情况下显示窗口
        /// </summary>
        public static void ShowWindowNoActivate(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle == IntPtr.Zero)
                    return;

                ShowWindow(windowHandle, SW_SHOWNOACTIVATE);
                SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 在不抢占焦点的情况下最小化窗口
        /// </summary>
        public static void MinimizeWindowNoActivate(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle == IntPtr.Zero)
                    return;

                ShowWindow(windowHandle, SW_SHOWMINNOACTIVE);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 判断窗口是否属于当前进程（过滤自身子窗口）
        /// </summary>
        public static bool IsCurrentProcessWindow(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle == IntPtr.Zero)
                    return false;

                GetWindowThreadProcessId(windowHandle, out uint processId);
                int currentProcessId = Process.GetCurrentProcess().Id;
                return processId == currentProcessId;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断指定窗口是否是 Photoshop 窗口
        /// </summary>
        public static bool IsPhotoshopWindow(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle == IntPtr.Zero)
                    return false;

                GetWindowThreadProcessId(windowHandle, out uint processId);
                try
                {
                    Process process = Process.GetProcessById((int)processId);
                    string processName = process.ProcessName.ToLower();
                    if (processName.Contains("photoshop"))
                    {
                        return true;
                    }
                }
                catch
                {
                }

                int length = GetWindowTextLength(windowHandle);
                if (length > 0)
                {
                    StringBuilder sb = new StringBuilder(length + 1);
                    GetWindowText(windowHandle, sb, sb.Capacity);
                    string title = sb.ToString();

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
