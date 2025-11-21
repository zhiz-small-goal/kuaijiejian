using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kuaijiejian
{
    /// <summary>
    /// Photoshop 动作选择窗口
    /// 基于 Adobe 官方 API 检测和选择动作
    /// </summary>
    public partial class ActionSelectorWindow : Window
    {
        private List<ActionItemViewModel> _actionItems;
        private List<ActionItemViewModel> _allActionItems; // 存储所有动作用于搜索

        /// <summary>
        /// 选中的动作列表
        /// </summary>
        public List<PhotoshopActionInfo> SelectedActions { get; private set; }

        // 定义事件：当用户确认添加动作时触发
        public event EventHandler<List<PhotoshopActionInfo>>? ActionsConfirmed;

        public ActionSelectorWindow()
        {
            InitializeComponent();
            _actionItems = new List<ActionItemViewModel>();
            SelectedActions = new List<PhotoshopActionInfo>();

            // 窗口加载后异步加载动作
            Loaded += ActionSelectorWindow_Loaded;
        }

        /// <summary>
        /// 拖拽窗口：点击窗口任意位置可拖动
        /// </summary>
        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 窗口加载事件
        /// </summary>
        private async void ActionSelectorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // 检查 Photoshop 是否安装
                    if (!PhotoshopHelper.IsPhotoshopInstalled())
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ShowError("未检测到 Photoshop，请确保 Photoshop 已正确安装。");
                        });
                        return;
                    }

                    // 获取所有动作（Adobe 官方方法）
                    System.Diagnostics.Debug.WriteLine("开始获取 Photoshop 动作...");
                    var actions = PhotoshopHelper.GetAllActions();
                    System.Diagnostics.Debug.WriteLine($"获取到 {actions.Count} 个动作");

                    Dispatcher.Invoke(() =>
                    {
                        LoadingPanel.Visibility = Visibility.Collapsed;

                        if (actions.Count == 0)
                        {
                            // 显示更详细的错误信息
                            EmptyPanel.Visibility = Visibility.Visible;
                            
                            // 根据错误类型显示不同的提示
                            var result = System.Windows.MessageBox.Show(
                                "未检测到 Photoshop 动作。\n\n" +
                                "可能的原因：\n" +
                                "1. 您的 Photoshop 版本可能不支持脚本自动检测\n" +
                                "2. Actions 面板中没有加载任何动作集\n" +
                                "3. Photoshop 权限设置阻止了脚本执行\n\n" +
                                "解决方案：\n" +
                                "• 在 Photoshop 中打开 Window → Actions\n" +
                                "• 点击面板菜单 → Load Actions 加载动作文件\n" +
                                "• 或者点击'是'手动添加动作（需要输入动作名称）\n\n" +
                                "是否要手动添加动作？",
                                "动作检测失败",
                                System.Windows.MessageBoxButton.YesNo,
                                System.Windows.MessageBoxImage.Question);
                            
                            if (result == System.Windows.MessageBoxResult.Yes)
                            {
                                // 用户选择手动添加，直接关闭窗口
                                Close();
                            }
                        }
                        else
                        {
                            // 转换为 ViewModel
                            foreach (var action in actions)
                            {
                                _actionItems.Add(new ActionItemViewModel
                                {
                                    ActionInfo = action,
                                    IsSelected = false
                                });
                            }

                            // 反转列表，使最新添加的动作显示在顶部
                            _actionItems.Reverse();
                            
                            _allActionItems = new List<ActionItemViewModel>(_actionItems); // 保存完整列表用于搜索
                            ActionsListBox.ItemsSource = _actionItems;
                            UpdateSelectedCount();
                            
                            // 自动聚焦搜索框
                            SearchBox.Focus();
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载动作异常：{ex}");
                    Dispatcher.Invoke(() =>
                    {
                        ShowError($"加载动作失败：{ex.Message}\n\n详细信息：{ex.StackTrace}");
                    });
                }
            });
        }

        /// <summary>
        /// 全选
        /// </summary>
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _actionItems)
            {
                item.IsSelected = true;
            }
            ActionsListBox.Items.Refresh();
            UpdateSelectedCount();
        }

        /// <summary>
        /// 取消全选
        /// </summary>
        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _actionItems)
            {
                item.IsSelected = false;
            }
            ActionsListBox.Items.Refresh();
            UpdateSelectedCount();
        }

        /// <summary>
        /// 复选框状态改变
        /// </summary>
        private void ActionCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCount();
        }

        /// <summary>
        /// 更新选中数量显示
        /// </summary>
        private void UpdateSelectedCount()
        {
            int count = _actionItems.Count(a => a.IsSelected);
            CountTextBlock.Text = $"已选择: {count}";
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedActions = _actionItems
                .Where(a => a.IsSelected)
                .Select(a => a.ActionInfo)
                .ToList();

            if (SelectedActions.Count == 0)
            {
                NotificationWindow.Show("💡 提示", "请至少选择一个动作", 0.5);
                return;
            }

            // 触发事件通知主窗口
            ActionsConfirmed?.Invoke(this, SelectedActions);
            Close();
        }

        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 搜索框文本变化事件 - 实时过滤动作列表
        /// </summary>
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_allActionItems == null) return;

            string searchText = SearchBox.Text.ToLower().Trim();
            
            // 控制清除按钮显示
            ClearSearchButton.Visibility = string.IsNullOrWhiteSpace(searchText) ? Visibility.Collapsed : Visibility.Visible;
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // 显示所有动作
                _actionItems = new List<ActionItemViewModel>(_allActionItems);
                ActionsListBox.ItemsSource = _actionItems;
            }
            else
            {
                // 过滤动作：搜索动作名称和动作集名称
                _actionItems = _allActionItems.Where(a => 
                    a.DisplayName.ToLower().Contains(searchText) ||
                    (a.ActionInfo.ActionSetName != null && a.ActionInfo.ActionSetName.ToLower().Contains(searchText)) ||
                    (a.ActionInfo.ActionName != null && a.ActionInfo.ActionName.ToLower().Contains(searchText))
                ).ToList();
                
                ActionsListBox.ItemsSource = _actionItems;
            }
            
            UpdateSelectedCount();
        }

        /// <summary>
        /// 清除搜索按钮点击事件
        /// </summary>
        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            SearchBox.Focus();
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        private void ShowError(string message)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            EmptyPanel.Visibility = Visibility.Collapsed;
            
            MessageBox.Show(message, "错误", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            
            Close();
        }
    }

    /// <summary>
    /// 动作项 ViewModel
    /// </summary>
    public class ActionItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        /// <summary>
        /// 动作信息
        /// </summary>
        public PhotoshopActionInfo ActionInfo { get; set; } = new PhotoshopActionInfo();

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName => ActionInfo.DisplayName;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

