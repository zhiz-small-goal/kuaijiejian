using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WinForms = System.Windows.Forms; // 系统托盘，使用别名避免冲突

namespace Kuaijiejian
{
    public partial class MainWindow : Window
    {
        private SettingsWindow? _settingsWindow;
        public FunctionManager _functionManager;
        
        // 拖拽排序（WPF官方最佳实践）
        private Point _dragStartPoint;
        private bool _isDragging = false;
        
        // 当前选中的按钮（用于F2重命名）
        private System.Windows.Controls.Button? _selectedButton;
        private FunctionItem? _selectedFunction;
        
        // Photoshop窗口监控定时器（方案1：定时器轮询）
        private DispatcherTimer? _photoshopMonitorTimer;
        
        // 窗口拖动状态标志（防止定时器在拖动时干扰）
        private bool _isWindowDragging = false;

        // 最近一次检测到 Photoshop 处于前台的时间（用于防抖）
        private DateTime _lastPhotoshopActiveTime = DateTime.MinValue;

        
        // UpdateWindowClip防抖定时器（防止频繁触发）
        private DispatcherTimer? _clipUpdateTimer;
        
        // 子窗口引用（用于关闭时清理）
        private LayerFunctionSelectorWindow? _layerFunctionSelector;
        private ActionSelectorWindow? _actionSelector;
        private BatchDeleteWindow? _batchDeleteWindow;
        
        // 系统托盘图标
        private WinForms.NotifyIcon? _notifyIcon;
        private bool _hasShownTrayNotification = false;

        public MainWindow()
        {
            InitializeComponent();
            
            // 启用高质量渲染，修复圆角显示问题
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            this.UseLayoutRounding = true;
            this.SnapsToDevicePixels = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            
            // 设置初始最小宽度（在加载配置前使用默认值）
            this.MinWidth = 150;
            
            // 初始化功能管理器
            _functionManager = new FunctionManager();
            
            // 绑定数据 - 统一列表
            AllFunctionsList.ItemsSource = _functionManager.AllFunctions;
            
            // 窗口初始化代码
            this.Loaded += MainWindow_Loaded;
            
            // 初始化系统托盘
            InitializeSystemTray();
            
            // 确保窗口正常显示
            this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Visible;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置窗口位置到屏幕顶部居中
            SetWindowPositionToTopRight();
            
            // 加载显示模式配置
            DisplayModeManager.LoadMode();
            RefreshAllButtonsDisplay();
            
            // 应用按钮布局配置
            ApplyButtonLayoutConfig();
            
            // 应用保存的主题
            ApplyTheme(ThemeManager.CurrentTheme);
            
            // 【焦点优化】设置窗口为不激活样式（点击按钮不抢焦点）
            // 这样可以保持Photoshop的焦点，不打断工作流
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            WindowsApiHelper.SetWindowNoActivate(helper.Handle);
            
            // 确保窗口激活并显示在前台（仅首次启动）
            this.Activate();
            
            // 性能优化：后台预热Photoshop COM连接
            // 确保用户第一次点击按钮时立即响应，无延迟
            System.Threading.Tasks.Task.Run(() => PhotoshopHelper.WarmUpConnection());
            this.Focus();
            
            // 初始化圆角裁剪
            UpdateWindowClip();
            
            // 添加F2快捷键监听
            this.KeyDown += MainWindow_KeyDown;
            
            // 【窗口跟随】初始化Photoshop窗口监控（方案1：定时器轮询）
            // 当Photoshop激活时自动显示，切换到其他程序时自动最小化
            InitializePhotoshopMonitor();
            
            UpdateFunctionCountInTitle();
            UpdateStatus($"程序已启动，共{_functionManager.AllFunctions.Count}个功能");
        }

        /// <summary>
        /// 设置窗口位置到屏幕顶部居中
        /// 自动适应不同分辨率的屏幕
        /// </summary>
        private void SetWindowPositionToTopRight()
        {
            // 使用 Dispatcher 延迟定位，确保窗口内容完全渲染后再计算位置
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                // 获取主屏幕工作区域（排除任务栏）
                var workingArea = System.Windows.SystemParameters.WorkArea;
                
                // 确保窗口已经渲染，获取实际大小
                this.UpdateLayout();
                
                // 计算位置：屏幕顶部居中
                double topMargin = 180;
                
                // 水平居中
                this.Left = workingArea.Left + (workingArea.Width - this.ActualWidth) / 2;
                // 垂直位置保持不变
                this.Top = workingArea.Top + topMargin;
                
                // 确保窗口不会超出屏幕边界
                if (this.Left < workingArea.Left)
                    this.Left = workingArea.Left + 20;
                
                if (this.Top < workingArea.Top)
                    this.Top = workingArea.Top + 20;
                    
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        
        /// <summary>
        /// 全局按键监听 - F2 重命名
        /// </summary>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2 && _selectedFunction != null)
            {
                RenameFunction(_selectedFunction);
                e.Handled = true;
            }
        }

        /// <summary>
        /// 窗口大小改变时更新裁剪区域，确保真正的圆角
        /// 使用防抖机制避免频繁触发
        /// </summary>
        private void WindowBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 使用防抖定时器，避免在窗口移动/调整大小时频繁触发
            if (_clipUpdateTimer == null)
            {
                _clipUpdateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(50)
                };
                _clipUpdateTimer.Tick += (s, args) =>
                {
                    _clipUpdateTimer?.Stop();
                    UpdateWindowClip();
                };
            }
            
            _clipUpdateTimer.Stop();
            _clipUpdateTimer.Start();
        }

        /// <summary>
        /// 更新窗口的圆角裁剪区域
        /// </summary>
        private void UpdateWindowClip()
        {
            if (WindowBorder.ActualWidth > 0 && WindowBorder.ActualHeight > 0)
            {
                var radius = 15.0;
                var clip = new RectangleGeometry(
                    new Rect(0, 0, WindowBorder.ActualWidth, WindowBorder.ActualHeight),
                    radius, radius);
                WindowBorder.Clip = clip;
            }
        }

        /// <summary>
        /// 应用主题配色 - 支持完整UI元素配色
        /// </summary>
        public void ApplyTheme(ColorTheme theme)
        {
            try
            {
                // 转换颜色
                var converter = new BrushConverter();
                
                // 窗口背景
                WindowBorder.Background = (Brush)converter.ConvertFromString(theme.WindowBackground)!;
                
                // 主区域（标题栏已移除）
                MainAreaStart.Color = (Color)ColorConverter.ConvertFromString(theme.LeftAreaStart);
                MainAreaEnd.Color = (Color)ColorConverter.ConvertFromString(theme.RightAreaEnd);
                MainSectionTitle.Foreground = (Brush)converter.ConvertFromString(theme.SectionTitleColor)!;
                
                // === 应用按钮主题颜色到动态资源 ===
                
                // 功能按钮颜色
                this.Resources["ButtonBackgroundBrush"] = (Brush)converter.ConvertFromString(theme.ButtonBackground)!;
                this.Resources["ButtonBorderBrush"] = (Brush)converter.ConvertFromString(theme.ButtonBorder)!;
                this.Resources["ButtonHoverBackgroundBrush"] = (Brush)converter.ConvertFromString(theme.ButtonHoverBackground)!;
                this.Resources["ButtonActiveBackgroundBrush"] = (Brush)converter.ConvertFromString(theme.ButtonActiveBackground)!;
                this.Resources["ButtonTextBrush"] = (Brush)converter.ConvertFromString(theme.ButtonTextColor)!;
                this.Resources["ButtonIconBrush"] = (Brush)converter.ConvertFromString(theme.ButtonIconColor)!;
                this.Resources["ButtonSubTextBrush"] = (Brush)converter.ConvertFromString(theme.ButtonSubTextColor)!;
                
                // 添加按钮颜色
                this.Resources["AddButtonBackgroundBrush"] = (Brush)converter.ConvertFromString(theme.AddButtonBackground)!;
                this.Resources["AddButtonBorderBrush"] = (Brush)converter.ConvertFromString(theme.AddButtonBorder)!;
                this.Resources["AddButtonHoverBackgroundBrush"] = (Brush)converter.ConvertFromString(theme.AddButtonHoverBackground)!;
                this.Resources["AddButtonIconBrush"] = (Brush)converter.ConvertFromString(theme.AddButtonIconColor)!;
                
                // 焦点框颜色
                this.Resources["FocusBrush"] = (Brush)converter.ConvertFromString(theme.FocusColor)!;
                
                UpdateStatus($"已应用「{theme.Name}」主题");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用主题失败：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region 窗口控制按钮

        /// <summary>
        /// 刷新所有按钮的显示文本
        /// </summary>
        public void RefreshAllButtonsDisplay()
        {
            // 刷新系统功能按钮
            foreach (var function in _functionManager.SystemFunctions)
            {
                function.RefreshDisplayText();
            }
            
            // 刷新应用功能按钮
            foreach (var function in _functionManager.ApplicationFunctions)
            {
                function.RefreshDisplayText();
            }
            
            // 更新标题显示的功能数量
            UpdateFunctionCountInTitle();
        }

        /// <summary>
        /// 更新标题中的功能数量
        /// </summary>
        private void UpdateFunctionCountInTitle()
        {
            int totalCount = _functionManager.AllFunctions.Count;
            int systemCount = _functionManager.SystemFunctions.Count;
            int actionCount = _functionManager.ApplicationFunctions.Count;
            
            MainSectionTitle.Text = $"功能 ({totalCount})";
        }

        /// <summary>
        /// 应用按钮布局配置（每行按钮数量）
        /// </summary>
        public void ApplyButtonLayoutConfig()
        {
            try
            {
                // 1. 计算并设置窗口宽度
                UpdateWindowWidth();
                
                // 2. 获取 UniformGrid（按钮网格容器）
                var itemsPanelTemplate = AllFunctionsList.ItemsPanel;
                
                // 强制刷新 ItemsPanel 以应用新的列数
                AllFunctionsList.Items.Refresh();
                AllFunctionsList.UpdateLayout();
                
                // 查找实际的 UniformGrid 实例
                if (AllFunctionsList.ItemsSource != null)
                {
                    var presenter = FindVisualChild<ItemsPresenter>(AllFunctionsList);
                    if (presenter != null)
                    {
                        presenter.ApplyTemplate();
                        var grid = FindVisualChild<System.Windows.Controls.Primitives.UniformGrid>(presenter);
                        if (grid != null)
                        {
                            grid.Columns = DisplayModeManager.ButtonsPerRow;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用按钮布局配置失败: {ex.Message}");
                // 静默失败，不影响主要功能
            }
        }

        /// <summary>
        /// 根据每行按钮数量自适应窗口宽度
        /// 使用异步方式避免阻塞UI线程
        /// </summary>
        private void UpdateWindowWidth()
        {
            try
            {
                int buttonsPerRow = DisplayModeManager.ButtonsPerRow;
                
                // 按钮固定宽度：45px
                // 按钮Margin：左右各4px，共8px
                // 每个按钮实际占用：45 + 8 = 53px
                double buttonWidth = 45;
                double buttonMargin = 8; // 左右各4px
                double buttonTotalWidth = buttonWidth + buttonMargin;
                
                // 计算按钮容器应该的宽度
                double containerWidth = buttonsPerRow * buttonTotalWidth;
                
                // 设置按钮容器宽度
                AllFunctionsList.Width = containerWidth;
                
                // === 设置主内容区域宽度（关键！）===
                // MainAreaBorder的Padding：左右18×2 = 36px
                // 按钮容器宽度 + Padding = MainAreaBorder实际内容宽度
                var mainAreaBorder = this.FindName("MainAreaBorder") as Border;
                if (mainAreaBorder != null)
                {
                    mainAreaBorder.Width = containerWidth + 36; // 36 = Padding左右18×2
                }
                
                // === 动态调整窗口最小宽度 ===
                // 窗口额外宽度：MainAreaBorder (Padding左右18×2 + Margin左右15×2)
                double windowExtraWidth = (18 + 15) * 2;  // = 66px
                double calculatedMinWidth = containerWidth + windowExtraWidth;
                
                // 设置动态最小宽度（完全跟随按钮数量）
                this.MinWidth = calculatedMinWidth;
                this.Width = calculatedMinWidth; // 同时设置实际宽度
                
                // 使用异步布局更新，避免阻塞UI线程（WPF最佳实践）
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.InvalidateMeasure();
                    this.InvalidateArrange();
                }), DispatcherPriority.Background);
                
                System.Diagnostics.Debug.WriteLine($"按钮数量/行: {buttonsPerRow}, 容器宽度: {containerWidth:F0}, MainAreaBorder宽度: {containerWidth + 36:F0}, 窗口宽度: {calculatedMinWidth:F0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新容器宽度失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 查找可视化树中的子元素
        /// </summary>
        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // 按住 Ctrl 键点击设置按钮 = 显示窗口检测报告（调试功能）
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                string report = DebugWindowDetector.DetectAllPhotoshopWindows();
                MessageBox.Show(report, "Photoshop 窗口检测报告", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // 如果设置窗口已经打开，则激活它
            if (_settingsWindow != null && _settingsWindow.IsLoaded)
            {
                _settingsWindow.Activate();
                _settingsWindow.Focus();
                return;
            }

            // 创建新的设置窗口
            _settingsWindow = new SettingsWindow(this);
            _settingsWindow.Owner = this;
            
            // 窗口关闭时清空引用
            _settingsWindow.Closed += (s, args) => _settingsWindow = null;
            
            // 使用 Show() 而不是 ShowDialog()，允许同时操作两个窗口
            _settingsWindow.Show();
        }

        /// <summary>
        /// 主区域鼠标按下事件 - 实现窗口拖动
        /// 使用try-catch和标志位避免与定时器冲突
        /// </summary>
        private void MainAreaBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    // 设置拖动标志，防止定时器在拖动时干扰
                    _isWindowDragging = true;
                    this.DragMove();
                }
                catch (InvalidOperationException)
                {
                    // 捕获DragMove可能抛出的异常（如在双击时调用）
                    // 静默处理，不影响用户体验
                }
                finally
                {
                    // 拖动结束后重置标志
                    _isWindowDragging = false;
                }
            }
        }
        
        #region 系统托盘功能
        
        /// <summary>
        /// 初始化系统托盘图标和菜单
        /// </summary>
        private void InitializeSystemTray()
        {
            _notifyIcon = new WinForms.NotifyIcon();
            _notifyIcon.Text = "PS Tools - Photoshop快捷工具";
            
            // 设置托盘图标（使用 app.ico）
            try
            {
                // 方法1：尝试从项目资源加载 app.ico
                var iconStream = System.Windows.Application.GetResourceStream(
                    new Uri("pack://application:,,,/app.ico", UriKind.Absolute))?.Stream;
                
                if (iconStream != null)
                {
                    _notifyIcon.Icon = new System.Drawing.Icon(iconStream);
                }
                else
                {
                    // 方法2：尝试从文件系统加载（适用于开发环境）
                    string iconPath = System.IO.Path.Combine(
                        System.AppDomain.CurrentDomain.BaseDirectory, 
                        "app.ico");
                    
                    if (System.IO.File.Exists(iconPath))
                    {
                        _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                    }
                    else
                    {
                        // 如果都找不到，使用系统默认图标
                        _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                    }
                }
            }
            catch
            {
                // 如果加载失败，使用系统默认图标
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }
            
            // 双击托盘图标显示/隐藏窗口
            _notifyIcon.DoubleClick += (s, e) => 
            {
                if (this.IsVisible)
                {
                    this.Hide();
                }
                else
                {
                    this.Show();
                    this.Activate();
                }
            };
            
            // 创建右键菜单
            var contextMenu = new WinForms.ContextMenuStrip();
            
            // 显示/隐藏
            var showHideItem = new WinForms.ToolStripMenuItem("显示主窗口");
            showHideItem.Click += (s, e) =>
            {
                if (this.IsVisible)
                {
                    this.Hide();
                }
                else
                {
                    this.Show();
                    this.Activate();
                    this.WindowState = WindowState.Normal;
                }
            };
            contextMenu.Items.Add(showHideItem);
            
            contextMenu.Items.Add(new WinForms.ToolStripSeparator());
            
            // 设置
            var settingsItem = new WinForms.ToolStripMenuItem("⚙️ 设置");
            settingsItem.Click += (s, e) =>
            {
                // 确保主窗口显示
                if (!this.IsVisible)
                {
                    this.Show();
                    this.Activate();
                }
                
                // 打开设置窗口
                SettingsButton_Click(this, new RoutedEventArgs());
            };
            contextMenu.Items.Add(settingsItem);
            
            // 窗口置顶
            var topmostItem = new WinForms.ToolStripMenuItem("📌 窗口置顶");
            topmostItem.Checked = this.Topmost;
            topmostItem.CheckOnClick = true;
            topmostItem.Click += (s, e) =>
            {
                this.Topmost = !this.Topmost;
                topmostItem.Checked = this.Topmost;
                UpdateStatus(this.Topmost ? "窗口已置顶" : "已取消窗口置顶");
            };
            contextMenu.Items.Add(topmostItem);
            
            // 开机启动
            var startupItem = new WinForms.ToolStripMenuItem("🚀 开机启动");
            startupItem.Checked = IsStartupEnabled();
            startupItem.CheckOnClick = true;
            startupItem.Click += (s, e) =>
            {
                if (startupItem.Checked)
                {
                    EnableStartup();
                    UpdateStatus("已启用开机启动");
                }
                else
                {
                    DisableStartup();
                    UpdateStatus("已禁用开机启动");
                }
            };
            contextMenu.Items.Add(startupItem);
            
            contextMenu.Items.Add(new WinForms.ToolStripSeparator());
            
            // 退出
            var exitItem = new WinForms.ToolStripMenuItem("退出");
            exitItem.Click += (s, e) => ExitApplication();
            contextMenu.Items.Add(exitItem);
            
            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.Visible = true;
        }
        
        /// <summary>
        /// 窗口关闭事件 - 隐藏到系统托盘而不是退出
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 取消关闭，隐藏窗口到系统托盘
            e.Cancel = true;
            this.Hide();
            
            // 显示托盘提示（仅首次）
            if (!_hasShownTrayNotification)
            {
                _notifyIcon?.ShowBalloonTip(2000, "PS Tools", "程序已最小化到系统托盘，双击托盘图标可重新显示", WinForms.ToolTipIcon.Info);
                _hasShownTrayNotification = true;
            }
        }
        
        /// <summary>
        /// 真正退出程序
        /// </summary>
        private void ExitApplication()
        {
            // 立即释放COM资源
            PhotoshopHelper.ReleaseCachedResources();
            
            // 清理系统托盘图标
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // 确保完全退出程序
            System.Windows.Application.Current.Shutdown();
            
            // 最后保险：强制退出进程
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        
        #endregion
        
        protected override void OnClosed(EventArgs e)
        {
            // 窗口完全关闭后再次确认资源释放
            PhotoshopHelper.ReleaseCachedResources();
            base.OnClosed(e);
            
            // 延迟强制退出（给其他清理代码执行机会）
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ => 
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            });
        }
        
        /// <summary>
        /// 检查是否已启用开机启动
        /// </summary>
        private bool IsStartupEnabled()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    return key?.GetValue("PSTools") != null;
                }
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 启用开机启动
        /// </summary>
        private void EnableStartup()
        {
            try
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? 
                                System.Reflection.Assembly.GetExecutingAssembly().Location;
                
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key?.SetValue("PSTools", $"\"{exePath}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机启动失败：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 禁用开机启动
        /// </summary>
        private void DisableStartup()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key?.DeleteValue("PSTools", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"取消开机启动失败：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        #endregion
        
        /// <summary>
        /// 更新状态栏信息（状态栏已移除，仅保留方法避免引用错误）
        /// </summary>
        private void UpdateStatus(string message)
        {
            // 状态栏已移除，保留此方法仅用于避免引用错误
            // 可用于调试：Debug.WriteLine($"{DateTime.Now:HH:mm:ss} - {message}");
        }

        /// <summary>
        /// 安全截取字符串的指定长度
        /// C# 官方最佳实践：防止索引越界
        /// </summary>
        /// <param name="text">源字符串</param>
        /// <param name="startIndex">起始索引</param>
        /// <param name="length">截取长度</param>
        /// <returns>截取后的字符串</returns>
        private string GetSafeSubstring(string text, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            
            if (startIndex >= text.Length)
                return string.Empty;
            
            // 计算实际可截取的长度
            int actualLength = Math.Min(length, text.Length - startIndex);
            
            return text.Substring(startIndex, actualLength);
        }

        #region 系统功能按钮事件

        /// <summary>
        /// 功能按钮点击事件 - 即点即执行
        /// </summary>
        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is FunctionItem function)
            {
                // 记录选中的按钮（用于F2重命名）
                _selectedButton = button;
                _selectedFunction = function;
                
                // 直接执行功能，无延迟
                ExecuteFunction(function);
            }
        }

        /// <summary>
        /// 执行功能
        /// </summary>
        private void ExecuteFunction(FunctionItem function)
        {
            try
            {
                UpdateStatus($"执行：{function.Name}");
                
                // 根据功能类型执行不同的操作
                switch (function.FunctionType)
                {
                    case "PhotoshopScript":
                        ExecutePhotoshopScript(function);
                        break;
                    
                    case "PhotoshopAction":
                        ExecutePhotoshopAction(function);
                        break;
                    
                    case "Normal":
                    default:
                        ExecuteNormalCommand(function);
                        break;
                }
                
                // 移除执行成功提示，避免遮挡工作界面
            }
            catch (Exception ex)
            {
                // 检查是否是用户取消操作（官方最佳实践：区分用户操作和系统错误）
                string errorMessage = ex.Message.ToLower();
                bool isUserCancelled = errorMessage.Contains("取消") || 
                                      errorMessage.Contains("cancel") || 
                                      errorMessage.Contains("用户");
                
                if (isUserCancelled)
                {
                    // 用户主动取消，只在状态栏提示，不显示错误框
                    UpdateStatus($"操作已取消：{function.Name}");
                }
                else
                {
                    // 真正的错误，显示错误对话框
                    MessageBox.Show($"执行失败：{ex.Message}", "错误", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"执行失败：{function.Name}");
                }
            }
        }
        
        /// <summary>
        /// 执行普通命令
        /// </summary>
        private void ExecuteNormalCommand(FunctionItem function)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = function.Command,
                UseShellExecute = true
            };
            
            Process.Start(startInfo);
        }
        
        /// <summary>
        /// 执行Photoshop脚本
        /// Adobe官方推荐：使用DoJavaScript执行脚本
        /// </summary>
        private void ExecutePhotoshopScript(FunctionItem function)
        {
            if (!PhotoshopHelper.IsPhotoshopInstalled())
            {
                throw new Exception("未检测到Photoshop，请确保已正确安装。");
            }
            
            // function.Command 可以是JSX脚本代码或JSX文件路径
            string scriptCode;
            if (System.IO.File.Exists(function.Command))
            {
                // 读取文件内容
                scriptCode = System.IO.File.ReadAllText(function.Command);
            }
            else
            {
                // 直接使用命令字符串作为脚本
                scriptCode = function.Command;
            }
            
            // 统一使用静默执行，无弹窗
            PhotoshopHelper.ExecuteScriptSilently(scriptCode);
        }
        
        /// <summary>
        /// 执行Photoshop动作
        /// Adobe官方方法：app.doAction(actionName, actionSetName)
        /// </summary>
        private void ExecutePhotoshopAction(FunctionItem function)
        {
            if (!PhotoshopHelper.IsPhotoshopInstalled())
            {
                throw new Exception("未检测到Photoshop，请确保已正确安装。");
            }
            
            if (string.IsNullOrEmpty(function.ActionName) || string.IsNullOrEmpty(function.ActionSetName))
            {
                throw new Exception("动作名称或动作集名称未设置。");
            }
            
            PhotoshopHelper.PlayAction(function.ActionName, function.ActionSetName);
        }

        /// <summary>
        /// 重命名功能（独立方法，供F2和菜单调用）
        /// </summary>
        private void RenameFunction(FunctionItem function)
        {
            var inputDialog = new InputDialog("重命名功能", function.Name)
            {
                Owner = this
            };
            
            if (inputDialog.ShowDialog() == true)
            {
                string newName = inputDialog.InputText;
                if (!string.IsNullOrEmpty(newName) && newName != function.Name)
                {
                    function.Name = newName;
                    _functionManager.SaveFunctions();
                    
                    // 刷新界面
                    AllFunctionsList.Items.Refresh();
                    
                    UpdateStatus($"已重命名为：{newName}");
                }
            }
        }

        /// <summary>
        /// 功能按钮右键点击事件 - 显示上下文菜单
        /// Windows 标准：右键菜单
        /// </summary>
        private void FunctionButton_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && button.Tag is FunctionItem function)
            {
                // 记录选中的按钮
                _selectedButton = button;
                _selectedFunction = function;
                
                // 创建上下文菜单
                var contextMenu = new ContextMenu();
                
                // 菜单项：设置颜色
                var colorItem = new MenuItem
                {
                    Header = "🎨 设置颜色",
                    FontSize = 13
                };
                colorItem.Click += (s, args) =>
                {
                    // 打开颜色选择器
                    var colorPicker = new ColorPickerWindow
                    {
                        Owner = this,
                        OnColorPreview = (hexColor) =>
                        {
                            // 实时预览：在主窗口按钮上显示颜色
                            function.CustomColor = hexColor;
                            AllFunctionsList.Items.Refresh();
                        }
                    };

                    if (colorPicker.ShowDialog() == true)
                    {
                        // 设置自定义颜色
                        function.CustomColor = colorPicker.SelectedColor;
                        
                        // 保存配置
                        _functionManager.SaveFunctions();
                        
                        // 刷新UI
                        AllFunctionsList.Items.Refresh();
                        
                        // 显示通知
                        string message = function.CustomColor == null 
                            ? "已重置为主题色" 
                            : $"已设置为自定义颜色";
                        UpdateStatus(message);
                        NotificationWindow.ShowSuccess(message, 1.5);
                    }
                };
                contextMenu.Items.Add(colorItem);
                
                // 分隔符
                contextMenu.Items.Add(new Separator());
                
                // 菜单项：重命名
                var renameItem = new MenuItem
                {
                    Header = "✏️ 重命名 (F2)",
                    FontSize = 13
                };
                renameItem.Click += (s, args) => RenameFunction(function);
                contextMenu.Items.Add(renameItem);
                
                // 分隔符
                contextMenu.Items.Add(new Separator());
                
                // 菜单项：删除
                var deleteItem = new MenuItem
                {
                    Header = "🗑️ 删除",
                    FontSize = 13
                };
                deleteItem.Click += (s, args) =>
                {
                    string functionName = function.Name;
                    _functionManager.RemoveFunction(function);
                    UpdateStatus($"已删除：{functionName}");
                };
                contextMenu.Items.Add(deleteItem);
                
                // 显示菜单
                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
                
                e.Handled = true;
            }
        }
        
        /// <summary>
        /// 功能按钮双击事件（已禁用，使用右键菜单+F2重命名）
        /// </summary>
        private void FunctionButton_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 双击时不做任何操作，避免重复执行
            e.Handled = true;
        }

        /// <summary>
        /// 添加系统功能按钮点击事件
        /// </summary>
        private void AddSystemFunction_Click(object sender, RoutedEventArgs e)
        {
            // 检查 Photoshop 是否安装
            if (!PhotoshopHelper.IsPhotoshopInstalled())
            {
                NotificationWindow.Show("⚠️ 警告", "未检测到 Photoshop，请确保已正确安装", 1.0);
                return;
            }

            // 打开图层功能选择窗口（非模态）
            _layerFunctionSelector = new LayerFunctionSelectorWindow
            {
                Owner = this
            };

            // 订阅确认事件
            _layerFunctionSelector.FunctionsConfirmed += (s, selectedFunctions) =>
            {
                // 添加选中的图层功能到主页面
                int addedCount = 0;
                foreach (var func in selectedFunctions)
                {
                    // 固定使用文字模式：显示功能名称前2个字
                    string displayText = GetSafeSubstring(func.DisplayName, 0, 2);
                    
                    var newFunction = new FunctionItem
                    {
                        Name = func.DisplayName,
                        Icon = displayText,
                        Hotkey = "",
                        Command = func.Script,
                        Category = "System",
                        FunctionType = "PhotoshopScript"
                    };

                    _functionManager.AddFunction(newFunction);
                    addedCount++;
                }

                if (addedCount > 0)
                {
                    UpdateFunctionCountInTitle();
                    UpdateStatus($"已添加 {addedCount} 个图层编辑功能");
                }
            };

            // 窗口关闭时清除引用（避免窗口跟随功能误判）
            _layerFunctionSelector.Closed += (s, e) => _layerFunctionSelector = null;

            // 显示窗口（非模态）
            _layerFunctionSelector.Show();
        }

        /// <summary>
        /// 添加应用功能按钮点击事件
        /// 右侧"动作"区 - 使用 Adobe 官方 API 检测并选择 Photoshop 动作
        /// 如果自动检测失败，提供手动添加选项
        /// </summary>
        private void AddApplicationFunction_Click(object sender, RoutedEventArgs e)
        {
            // 检查 Photoshop 是否安装
            if (!PhotoshopHelper.IsPhotoshopInstalled())
            {
                NotificationWindow.Show("⚠️ 警告", "未检测到 Photoshop，请确保已正确安装", 1.0);
                return;
            }

            // 打开动作选择窗口（非模态）
            _actionSelector = new ActionSelectorWindow
            {
                Owner = this
            };

            // 订阅确认事件
            _actionSelector.ActionsConfirmed += (s, selectedActions) =>
            {
                // 添加选中的动作到主页面
                int addedCount = 0;
                foreach (var action in selectedActions)
                {
                    // 获取动作名称的前2个字符作为按钮文字
                    // 使用 C# 官方最佳实践：安全截取字符串
                    string buttonText = GetSafeSubstring(action.ActionName, 0, 2);
                    
                    var newFunction = new FunctionItem
                    {
                        Name = action.ActionName,  // 保存完整动作名称，显示在按钮下方小字
                        Icon = buttonText,  // 按钮主要显示区域：动作名称的前2个字
                        Hotkey = "",
                        Command = "",
                        Category = "Application",
                        FunctionType = "PhotoshopAction",
                        ActionSetName = action.ActionSetName,
                        ActionName = action.ActionName
                    };

                    _functionManager.AddFunction(newFunction);
                    addedCount++;
                }

                if (addedCount > 0)
                {
                    UpdateFunctionCountInTitle();
                    UpdateStatus($"已添加 {addedCount} 个 Photoshop 动作");
                }
            };

            // 窗口关闭时清除引用（避免窗口跟随功能误判）
            _actionSelector.Closed += (s, e) => _actionSelector = null;

            // 显示窗口（非模态）
            _actionSelector.Show();
        }

        /// <summary>
        /// 手动添加应用功能
        /// 直接打开手动添加窗口，无需 Photoshop 自动检测
        /// </summary>
        private void ManualAddApplicationFunction_Click(object sender, RoutedEventArgs e)
        {
            var manualWindow = new ManualActionWindow
            {
                Owner = this
            };

            if (manualWindow.ShowDialog() == true && manualWindow.ActionInfo != null)
            {
                var action = manualWindow.ActionInfo;
                
                // 获取动作名称的前2个字符作为按钮文字
                string buttonText = GetSafeSubstring(action.ActionName, 0, 2);
                
                var newFunction = new FunctionItem
                {
                    Name = action.ActionName,  // 保存完整动作名称
                    Icon = buttonText,  // 按钮主要显示：动作名称的前2个字
                    Hotkey = "",
                    Command = "",
                    Category = "Application",
                    FunctionType = "PhotoshopAction",
                    ActionSetName = action.ActionSetName,
                    ActionName = action.ActionName
                };

                _functionManager.AddFunction(newFunction);
                UpdateStatus($"已手动添加 Photoshop 动作：{action.ActionName}");
            }
        }

        /// <summary>
        /// 批量删除所有功能 - 多选删除
        /// </summary>
        private void BatchDeleteAllFunctions_Click(object sender, RoutedEventArgs e)
        {
            if (_functionManager.AllFunctions.Count == 0)
            {
                NotificationWindow.Show("💡 提示", "当前没有任何功能可删除", durationSeconds: 0.5);
                return;
            }

            // 创建批量删除窗口
            var batchDeleteWindow = new BatchDeleteWindow(_functionManager.AllFunctions)
            {
                Owner = this
            };
            
            // 保存引用用于窗口跟随功能判断
            _batchDeleteWindow = batchDeleteWindow;
            
            // 显示对话框
            bool? dialogResult = batchDeleteWindow.ShowDialog();
            
            // 窗口关闭后立即获取选中的项（在清除引用之前）
            var selectedFunctions = batchDeleteWindow.SelectedFunctions;
            
            // 清除引用（避免窗口跟随功能误判）
            _batchDeleteWindow = null;

            // 处理删除操作
            if (dialogResult == true && selectedFunctions != null && selectedFunctions.Count > 0)
            {
                foreach (var function in selectedFunctions)
                {
                    _functionManager.RemoveFunction(function);
                }

                UpdateFunctionCountInTitle();
                UpdateStatus($"已删除 {selectedFunctions.Count} 个功能");
            }
        }

        /// <summary>
        /// 一键清空所有功能
        /// </summary>
        private void ClearAllFunctions_Click(object sender, RoutedEventArgs e)
        {
            if (_functionManager.AllFunctions.Count == 0)
            {
                return;
            }

            // 直接清空，无需确认
            int count = _functionManager.AllFunctions.Count;
            _functionManager.ClearAllFunctions();

            UpdateFunctionCountInTitle();
            UpdateStatus($"已清空{count}个功能");
        }

        #endregion

        #region 拖拽排序（WPF官方最佳实践）

        private void FunctionButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void FunctionButton_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _isDragging)
                return;

            Point currentPosition = e.GetPosition(null);
            Vector diff = _dragStartPoint - currentPosition;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;

                var button = sender as Button;
                if (button?.Tag is FunctionItem draggedItem)
                {
                    DragDrop.DoDragDrop(button, draggedItem, DragDropEffects.Move);
                }

                _isDragging = false;
            }
        }

        private void FunctionButton_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(FunctionItem)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void FunctionButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(FunctionItem)))
            {
                var droppedItem = e.Data.GetData(typeof(FunctionItem)) as FunctionItem;
                var targetButton = sender as Button;
                var targetItem = targetButton?.Tag as FunctionItem;

                if (droppedItem != null && targetItem != null && droppedItem != targetItem)
                {
                    // 在 AllFunctions 中交换位置
                    int oldIndex = _functionManager.AllFunctions.IndexOf(droppedItem);
                    int newIndex = _functionManager.AllFunctions.IndexOf(targetItem);

                    if (oldIndex >= 0 && newIndex >= 0)
                    {
                        // 真正的交换：A和B直接对调位置
                        // 使用C# 7.0的元组语法进行交换
                        (_functionManager.AllFunctions[oldIndex], _functionManager.AllFunctions[newIndex]) = 
                            (_functionManager.AllFunctions[newIndex], _functionManager.AllFunctions[oldIndex]);
                        
                        // 同步更新到原始列表
                        UpdateOriginalCollectionsOrder();
                        _functionManager.SaveFunctions();
                    }
                }
            }
        }

        /// <summary>
        /// 根据 AllFunctions 的顺序更新 SystemFunctions 和 ApplicationFunctions
        /// </summary>
        private void UpdateOriginalCollectionsOrder()
        {
            _functionManager.SystemFunctions.Clear();
            _functionManager.ApplicationFunctions.Clear();

            foreach (var item in _functionManager.AllFunctions)
            {
                if (item.Category == "System")
                {
                    _functionManager.SystemFunctions.Add(item);
                }
                else
                {
                    _functionManager.ApplicationFunctions.Add(item);
                }
            }
        }

        #endregion

        #region 窗口跟随Photoshop（方案1：定时器轮询）

        /// <summary>
        /// 初始化Photoshop窗口监控
        /// 使用定时器轮询方式检测Photoshop是否是前台窗口
        /// </summary>
        private void InitializePhotoshopMonitor()
        {
            try
            {
                // 创建定时器，每500ms检测一次
                _photoshopMonitorTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                
                _photoshopMonitorTimer.Tick += PhotoshopMonitorTimer_Tick;
                
                // 根据配置决定是否启动定时器
                if (DisplayModeManager.EnableWindowFollow)
                {
                    _photoshopMonitorTimer.Start();
                    UpdateStatus("窗口跟随已启用：Photoshop激活时自动显示，切换到其他程序时自动隐藏");
                }
                else
                {
                    UpdateStatus("窗口跟随已禁用：程序窗口保持置顶显示");
                }
            }
            catch (Exception ex)
            {
                // 静默失败，不影响程序主功能
                Debug.WriteLine($"初始化窗口监控失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新Photoshop窗口监控状态（响应设置更改）
        /// </summary>
        public void UpdatePhotoshopMonitorState()
        {
            try
            {
                if (_photoshopMonitorTimer == null) return;
                
                if (DisplayModeManager.EnableWindowFollow)
                {
                    // 启用窗口跟随
                    if (!_photoshopMonitorTimer.IsEnabled)
                    {
                        _photoshopMonitorTimer.Start();
                        UpdateStatus("窗口跟随已启用");
                    }
                }
                else
                {
                    // 禁用窗口跟随
                    if (_photoshopMonitorTimer.IsEnabled)
                    {
                        _photoshopMonitorTimer.Stop();
                        
                        // 确保窗口显示（不会被隐藏）
                        if (this.WindowState == WindowState.Minimized)
                        {
                            this.WindowState = WindowState.Normal;
                        }
                        
                        UpdateStatus("窗口跟随已禁用");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"更新窗口监控状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 定时器Tick事件：检测Photoshop窗口状态并自动显示/隐藏（带防抖）
        /// </summary>
        private void PhotoshopMonitorTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // 如果正在拖拽或拖动窗口，不要最小化窗口（避免操作被打断）
                if (_isDragging || _isWindowDragging)
                    return;

                // 获取当前前台窗口
                IntPtr foregroundWindow = WindowsApiHelper.GetForegroundWindow();

                // 如果前台窗口属于当前进程（主窗口或任何子窗口/对话框），不要最小化
                if (WindowsApiHelper.IsCurrentProcessWindow(foregroundWindow))
                    return;

                // 检查是否是 Photoshop 窗口
                bool isPhotoshopActive = WindowsApiHelper.IsPhotoshopWindow(foregroundWindow);

                if (isPhotoshopActive)
                {
                    // 记录 Photoshop 最近一次处于前台的时间
                    _lastPhotoshopActiveTime = DateTime.Now;

                    // Photoshop 处于激活状态：显示程序窗口
                    if (this.WindowState == WindowState.Minimized)
                    {
                        this.WindowState = WindowState.Normal;
                        Debug.WriteLine("检测到 Photoshop 激活，自动显示窗口");
                    }
                }
                else
                {
                    // 前台不是 Photoshop：增加一个防抖时间，避免 Alt+Tab / 临时弹窗时频繁闪烁
                    if (_lastPhotoshopActiveTime != DateTime.MinValue &&
                        (DateTime.Now - _lastPhotoshopActiveTime).TotalMilliseconds < 200)
                    {
                        // 刚刚才从 Photoshop 切出来，不立刻隐藏，等下一次 Tick 再看
                        return;
                    }

                    // 其他程序处于激活状态：最小化到任务栏
                    if (this.WindowState != WindowState.Minimized)
                    {
                        this.WindowState = WindowState.Minimized;
                        Debug.WriteLine("检测到切换到其他程序，自动隐藏窗口");
                    }
                }
            }
            catch (Exception ex)
            {
                // 静默失败，不影响程序运行
                Debug.WriteLine($"窗口监控异常: {ex.Message}");
            }
        }


        #endregion
    }
}

