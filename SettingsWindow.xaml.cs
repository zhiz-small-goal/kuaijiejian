using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Kuaijiejian
{
    public partial class SettingsWindow : Window
    {
        public MainWindow? _mainWindow;

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            
            // 加载所有主题
            ThemeList.ItemsSource = ColorTheme.GetAllThemes();
            
            // 初始化窗口跟随选项
            InitializeWindowFollow();
            
            // 初始化按钮布局选项
            InitializeButtonLayout();
            
            // 窗口加载完成后初始化圆角裁剪
            this.Loaded += (s, e) => UpdateWindowClip();
        }

        /// <summary>
        /// 初始化窗口跟随选项
        /// </summary>
        private void InitializeWindowFollow()
        {
            WindowFollowCheckBox.IsChecked = DisplayModeManager.EnableWindowFollow;
        }

        /// <summary>
        /// 初始化按钮布局选项
        /// </summary>
        private void InitializeButtonLayout()
        {
            // 计算并设置滑块范围（基于总按钮数、最大行数和屏幕宽度）
            UpdateSliderMinimum();
            
            // 更新提示文字
            UpdateRangeHintText();
            
            int currentValue = DisplayModeManager.ButtonsPerRow;
            
            // 确保当前值在有效范围内
            currentValue = Math.Max((int)ButtonsPerRowSlider.Minimum, 
                                   Math.Min(currentValue, (int)ButtonsPerRowSlider.Maximum));
            
            ButtonsPerRowSlider.Value = currentValue;
            ButtonsPerRowInput.Text = currentValue.ToString();
            
            // 如果当前值被调整了，保存新值
            if (currentValue != DisplayModeManager.ButtonsPerRow)
            {
                DisplayModeManager.ButtonsPerRow = currentValue;
            }
        }

        /// <summary>
        /// 更新范围提示文字
        /// </summary>
        private void UpdateRangeHintText()
        {
            try
            {
                int min = (int)ButtonsPerRowSlider.Minimum;
                int max = (int)ButtonsPerRowSlider.Maximum;
                int totalButtons = _mainWindow?._functionManager?.AllFunctions?.Count ?? 0;
                
                // 计算实际行数
                int maxRowsByScreen = CalculateMaxRowsByScreenHeight();
                int actualRows = totalButtons > 0 && max > 0 ? (int)Math.Ceiling((double)totalButtons / max) : 0;
                
                SliderRangeHint.Text = $"📏 智能范围：{min}-{max} 个/行（屏幕最多{maxRowsByScreen}行，最多显示{max}个/行，当前{actualRows}行）";
            }
            catch
            {
                // 失败时使用默认文字
            }
        }

        /// <summary>
        /// 根据总按钮数和最大行数，计算并更新滑块的最小值和最大值
        /// </summary>
        private void UpdateSliderMinimum()
        {
            try
            {
                // 获取主窗口的按钮总数
                int totalButtons = _mainWindow?._functionManager?.AllFunctions?.Count ?? 0;
                
                // === 计算滑块最小值（基于行数限制） ===
                // 同时考虑：1. 固定的最大行数限制(40行)  2. 屏幕实际高度限制
                int minButtonsPerRowByConfig = 1;
                int minButtonsPerRowByScreen = 1;
                
                if (totalButtons > 0)
                {
                    // 1. 基于配置的最大行数限制（40行）
                    minButtonsPerRowByConfig = (int)Math.Ceiling((double)totalButtons / DisplayModeManager.MaxRows);
                    
                    // 2. 基于屏幕实际高度的限制
                    int maxRowsByScreen = CalculateMaxRowsByScreenHeight();
                    if (maxRowsByScreen > 0)
                    {
                        minButtonsPerRowByScreen = (int)Math.Ceiling((double)totalButtons / maxRowsByScreen);
                    }
                }
                
                // 取两者中的较大值（更严格的限制）
                int minButtonsPerRow = Math.Max(minButtonsPerRowByConfig, minButtonsPerRowByScreen);
                minButtonsPerRow = Math.Max(1, minButtonsPerRow);
                
                // === 计算滑块最大值（基于屏幕宽度限制） ===
                int maxButtonsPerRow = CalculateMaxButtonsPerRow();
                
                // 设置滑块范围
                ButtonsPerRowSlider.Minimum = minButtonsPerRow;
                ButtonsPerRowSlider.Maximum = maxButtonsPerRow;
                
                System.Diagnostics.Debug.WriteLine($"总按钮数: {totalButtons}, 配置行限制: 最小{minButtonsPerRowByConfig}个/行, 屏幕高度限制: 最小{minButtonsPerRowByScreen}个/行, 最终最小值: {minButtonsPerRow}个/行, 屏幕宽度限制: 最大{maxButtonsPerRow}个/行");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新滑块范围失败: {ex.Message}");
                ButtonsPerRowSlider.Minimum = 1;
                ButtonsPerRowSlider.Maximum = 100;
            }
        }

        /// <summary>
        /// 根据屏幕高度和DPI，计算最大可显示的行数
        /// </summary>
        private int CalculateMaxRowsByScreenHeight()
        {
            try
            {
                // 1. 获取DPI缩放比例
                double dpiScale = GetDpiScale();
                
                // 2. 计算实际显示的按钮高度（考虑DPI缩放）
                double baseButtonHeight = 25.0;  // 基础按钮高度
                double baseButtonMargin = 8.0;   // 基础按钮边距（上下各4px）
                double actualButtonHeight = (baseButtonHeight + baseButtonMargin) * dpiScale;
                
                // 3. 获取屏幕工作区高度（排除任务栏）
                double screenHeight = SystemParameters.WorkArea.Height;
                
                // 4. 计算窗口额外占用的高度
                // - 标题栏: 约50px
                // - 主区域标题栏: 约40px
                // - MainAreaBorder: Padding 上下各15px, Margin 上下各15px
                // - 状态栏: 约40px
                // - 窗口阴影和圆角: 约30px
                // - 安全边距: 50px（防止贴边和操作系统UI）
                double extraHeight = 50 + 40 + (15 + 15) * 2 + 40 + 30 + 50;
                double extraHeightScaled = extraHeight * dpiScale;
                
                // 5. 计算可用于显示按钮的高度
                double availableHeight = screenHeight - extraHeightScaled;
                
                // 6. 计算最大可显示的行数
                int maxRows = (int)(availableHeight / actualButtonHeight);
                
                // 7. 确保至少为1
                maxRows = Math.Max(1, maxRows);
                
                System.Diagnostics.Debug.WriteLine($"DPI缩放: {dpiScale:F2}x, 屏幕高度: {screenHeight:F0}px, 实际按钮高度: {actualButtonHeight:F1}px, 窗口额外高度: {extraHeightScaled:F0}px, 最大行数: {maxRows}");
                
                return maxRows;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"计算最大行数失败: {ex.Message}");
                return DisplayModeManager.MaxRows; // 失败时返回配置的最大行数
            }
        }

        /// <summary>
        /// 根据屏幕宽度和DPI，计算最大可显示的按钮数量
        /// </summary>
        private int CalculateMaxButtonsPerRow()
        {
            try
            {
                // 1. 获取DPI缩放比例
                double dpiScale = GetDpiScale();
                
                // 2. 计算实际显示的按钮宽度（考虑DPI缩放）
                double baseButtonWidth = 45.0;  // 基础按钮宽度
                double baseButtonMargin = 8.0;  // 基础按钮边距（左右各4px）
                double actualButtonWidth = (baseButtonWidth + baseButtonMargin) * dpiScale;
                
                // 3. 获取屏幕工作区宽度（排除任务栏）
                double screenWidth = SystemParameters.WorkArea.Width;
                
                // 4. 计算窗口额外占用的宽度
                // MainAreaBorder: Margin 左右各15px, Padding 左右各18px
                // 窗口阴影和圆角: 约20px
                // 安全边距: 100px（防止贴边）
                double extraWidth = (15 + 18) * 2 + 20 + 100;
                double extraWidthScaled = extraWidth * dpiScale;
                
                // 5. 计算可用于显示按钮的宽度
                double availableWidth = screenWidth - extraWidthScaled;
                
                // 6. 计算最大可显示的按钮数量
                int maxButtons = (int)(availableWidth / actualButtonWidth);
                
                // 7. 确保至少为1，最多为100
                maxButtons = Math.Max(1, Math.Min(maxButtons, 100));
                
                System.Diagnostics.Debug.WriteLine($"DPI缩放: {dpiScale:F2}x, 屏幕宽度: {screenWidth:F0}px, 实际按钮宽度: {actualButtonWidth:F1}px, 最大按钮数: {maxButtons}");
                
                return maxButtons;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"计算最大按钮数失败: {ex.Message}");
                return 100; // 失败时返回默认值
            }
        }

        /// <summary>
        /// 获取当前DPI缩放比例
        /// 缓存DPI值避免重复计算
        /// </summary>
        private double? _cachedDpiScale = null;
        
        private double GetDpiScale()
        {
            // 使用缓存值，避免重复计算
            if (_cachedDpiScale.HasValue)
            {
                return _cachedDpiScale.Value;
            }
            
            try
            {
                // 方法1：通过PresentationSource获取DPI
                var source = PresentationSource.FromVisual(this);
                if (source != null && source.CompositionTarget != null)
                {
                    double dpiX = source.CompositionTarget.TransformToDevice.M11;
                    _cachedDpiScale = dpiX;
                    return dpiX;
                }
                
                // 方法2：如果方法1失败，使用VisualTreeHelper（需要.NET Core 3.0+）
                var dpi = VisualTreeHelper.GetDpi(this);
                _cachedDpiScale = dpi.DpiScaleX;
                return dpi.DpiScaleX;
            }
            catch
            {
                // 如果都失败，假设100% DPI
                _cachedDpiScale = 1.0;
                return 1.0;
            }
        }

        /// <summary>
        /// 窗口跟随设置改变事件
        /// </summary>
        private void WindowFollow_Changed(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded) return; // 避免初始化时触发
            
            // 更新设置
            DisplayModeManager.EnableWindowFollow = WindowFollowCheckBox.IsChecked == true;
            
            // 通知主窗口更新定时器状态
            _mainWindow?.UpdatePhotoshopMonitorState();
            
            // 显示提示
            string message = WindowFollowCheckBox.IsChecked == true 
                ? "已启用窗口跟随（切换到其他程序时自动隐藏）" 
                : "已禁用窗口跟随（窗口保持置顶显示）";
            NotificationWindow.ShowSuccess(message, 2.0);
        }

        /// <summary>
        /// 每行按钮数量滑块改变事件
        /// 使用防抖机制避免频繁触发布局更新
        /// </summary>
        private DispatcherTimer? _sliderChangeTimer;
        
        private void ButtonsPerRowSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsLoaded) return; // 避免初始化时触发
            
            int value = (int)e.NewValue;
            
            // 同步更新输入框
            if (ButtonsPerRowInput != null && ButtonsPerRowInput.Text != value.ToString())
            {
                ButtonsPerRowInput.Text = value.ToString();
            }
            
            // 使用防抖定时器，避免拖动滑块时频繁更新布局
            if (_sliderChangeTimer == null)
            {
                _sliderChangeTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(150)
                };
                _sliderChangeTimer.Tick += (s, args) =>
                {
                    _sliderChangeTimer?.Stop();
                    int currentValue = (int)ButtonsPerRowSlider.Value;
                    ApplyButtonsPerRowValue(currentValue);
                };
            }
            
            _sliderChangeTimer.Stop();
            _sliderChangeTimer.Start();
        }

        /// <summary>
        /// 输入框文本改变事件
        /// </summary>
        private void ButtonsPerRowInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!this.IsLoaded) return; // 避免初始化时触发
            
            if (int.TryParse(ButtonsPerRowInput.Text, out int value))
            {
                // 限制范围
                int min = (int)ButtonsPerRowSlider.Minimum;
                int max = (int)ButtonsPerRowSlider.Maximum;
                value = Math.Max(min, Math.Min(value, max));
                
                // 同步更新滑块
                if (ButtonsPerRowSlider.Value != value)
                {
                    ButtonsPerRowSlider.Value = value;
                }
            }
        }

        /// <summary>
        /// 输入框只允许输入数字
        /// </summary>
        private void ButtonsPerRowInput_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // 只允许数字
            e.Handled = !int.TryParse(e.Text, out _);
        }

        /// <summary>
        /// 输入框获得焦点时全选文本
        /// </summary>
        private void ButtonsPerRowInput_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonsPerRowInput.SelectAll();
        }

        /// <summary>
        /// 应用按钮数量设置
        /// </summary>
        private void ApplyButtonsPerRowValue(int value)
        {
            // 保存配置
            DisplayModeManager.ButtonsPerRow = value;
            
            // 通知主窗口更新布局
            _mainWindow?.ApplyButtonLayoutConfig();
            
            // 计算当前会有多少行（保留用于后续可能的用途）
            // int totalButtons = _mainWindow?._functionManager?.AllFunctions?.Count ?? 0;
            // int rows = totalButtons > 0 ? (int)Math.Ceiling((double)totalButtons / value) : 0;
            
            // 已移除：滑动时的提示弹窗（避免频繁弹出造成卡顿）
            // NotificationWindow.ShowSuccess($"已设置为每行 {value} 个按钮（共 {rows} 行）", 1.5);
        }

        /// <summary>
        /// 窗口大小改变时更新裁剪区域
        /// 使用防抖机制避免频繁触发
        /// </summary>
        private System.Windows.Threading.DispatcherTimer? _clipUpdateTimer;
        
        private void SettingsBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 使用防抖定时器，避免频繁触发
            if (_clipUpdateTimer == null)
            {
                _clipUpdateTimer = new System.Windows.Threading.DispatcherTimer
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
            if (SettingsBorder.ActualWidth > 0 && SettingsBorder.ActualHeight > 0)
            {
                var radius = 12.0;
                var clip = new System.Windows.Media.RectangleGeometry(
                    new System.Windows.Rect(0, 0, SettingsBorder.ActualWidth, SettingsBorder.ActualHeight),
                    radius, radius);
                SettingsBorder.Clip = clip;
            }
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string themeName)
            {
                var theme = ColorTheme.GetAllThemes()
                    .FirstOrDefault(t => t.Name == themeName);
                
                if (theme != null)
                {
                    // 应用主题
                    ThemeManager.ApplyTheme(theme);
                    
                    // 刷新主窗口
                    _mainWindow?.ApplyTheme(theme);
                    
                    // 显示1秒的自定义通知
                    NotificationWindow.ShowThemeChanged(themeName, 1);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HelpScrollViewer != null)
            {
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    HelpScrollViewer.ScrollToTop();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void HelpTab_Loaded(object sender, RoutedEventArgs e)
        {
            if (HelpScrollViewer != null)
            {
                HelpScrollViewer.ScrollToTop();
            }
        }
    }
}

