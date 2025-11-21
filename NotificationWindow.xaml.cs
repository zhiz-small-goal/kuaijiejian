using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Kuaijiejian
{
    public partial class NotificationWindow : Window
    {
        private DispatcherTimer? _timer;

        public NotificationWindow(string title, string message, double durationSeconds = 1.0)
        {
            InitializeComponent();
            
            TitleText.Text = title;
            MessageText.Text = message;

            // 设置定时器自动关闭
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(durationSeconds);
            _timer.Tick += Timer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 定位到鼠标位置（右下偏移一点，避免挡住鼠标）
            GetCursorPos(out POINT point);
            this.Left = point.X + 10;
            this.Top = point.Y + 10;
            
            // 确保窗口不超出屏幕
            var workArea = SystemParameters.WorkArea;
            if (this.Left + this.ActualWidth > workArea.Right)
            {
                this.Left = workArea.Right - this.ActualWidth - 10;
            }
            if (this.Top + this.ActualHeight > workArea.Bottom)
            {
                this.Top = workArea.Bottom - this.ActualHeight - 10;
            }
            
            // 设置鼠标穿透
            SetWindowTransparent();
            
            // 初始化圆角裁剪
            UpdateWindowClip();
            
            // 淡入动画
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            this.BeginAnimation(OpacityProperty, fadeIn);
            
            // 启动定时器
            _timer?.Start();
        }
        
        /// <summary>
        /// 设置窗口鼠标穿透
        /// </summary>
        private void SetWindowTransparent()
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
        
        // Windows API 常量
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        
        // Windows API 导入
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);
        
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timer?.Stop();
            
            // 淡出动画
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.Completed += (s, e) => this.Close();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        /// <summary>
        /// 显示通知
        /// </summary>
        public static void Show(string title, string message, double durationSeconds = 1.0)
        {
            var notification = new NotificationWindow(title, message, durationSeconds);
            notification.Show();
        }

        /// <summary>
        /// 显示成功通知
        /// </summary>
        public static void ShowSuccess(string message, double durationSeconds = 1.0)
        {
            Show("✨ 成功", message, durationSeconds);
        }

        /// <summary>
        /// 显示主题切换通知
        /// </summary>
        public static void ShowThemeChanged(string themeName, double durationSeconds = 1.0)
        {
            Show("🎨 主题切换", $"已切换到「{themeName}」主题！", durationSeconds);
        }

        /// <summary>
        /// 窗口大小改变时更新裁剪区域
        /// </summary>
        private void NotificationBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWindowClip();
        }

        /// <summary>
        /// 更新窗口的圆角裁剪区域
        /// </summary>
        private void UpdateWindowClip()
        {
            if (NotificationBorder.ActualWidth > 0 && NotificationBorder.ActualHeight > 0)
            {
                var radius = 12.0;
                var clip = new System.Windows.Media.RectangleGeometry(
                    new System.Windows.Rect(0, 0, NotificationBorder.ActualWidth, NotificationBorder.ActualHeight),
                    radius, radius);
                NotificationBorder.Clip = clip;
            }
        }
    }
}

