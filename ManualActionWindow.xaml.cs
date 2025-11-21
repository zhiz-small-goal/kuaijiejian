using System;
using System.Windows;
using System.Windows.Controls;

namespace Kuaijiejian
{
    /// <summary>
    /// 手动添加 Photoshop 动作窗口
    /// 基于 WPF 官方最佳实践：实时验证、Placeholder、用户友好提示
    /// </summary>
    public partial class ManualActionWindow : Window
    {
        public PhotoshopActionInfo? ActionInfo { get; private set; }

        public ManualActionWindow()
        {
            InitializeComponent();
            
            // 初始化时聚焦到第一个输入框（UX 最佳实践）
            Loaded += (s, e) => ActionSetTextBox.Focus();
        }

        /// <summary>
        /// 动作集名称文本改变事件
        /// WPF 最佳实践：实时验证和反馈
        /// </summary>
        private void ActionSetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 控制 Placeholder 显示/隐藏（官方推荐方法）
            ActionSetPlaceholder.Visibility = string.IsNullOrEmpty(ActionSetTextBox.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            // 实时验证
            ValidateInputs();
            UpdatePreview();
        }

        /// <summary>
        /// 动作名称文本改变事件
        /// </summary>
        private void ActionNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 控制 Placeholder 显示/隐藏
            ActionNamePlaceholder.Visibility = string.IsNullOrEmpty(ActionNameTextBox.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            // 实时验证
            ValidateInputs();
            UpdatePreview();
        }

        /// <summary>
        /// 验证输入
        /// C# 官方最佳实践：表单验证模式
        /// </summary>
        private void ValidateInputs()
        {
            string actionSet = ActionSetTextBox.Text.Trim();
            string actionName = ActionNameTextBox.Text.Trim();
            
            bool isActionSetValid = !string.IsNullOrEmpty(actionSet);
            bool isActionNameValid = !string.IsNullOrEmpty(actionName);
            
            // 显示/隐藏错误提示
            ActionSetError.Visibility = !isActionSetValid && ActionSetTextBox.Text.Length > 0 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            ActionNameError.Visibility = !isActionNameValid && ActionNameTextBox.Text.Length > 0 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            // 控制添加按钮启用状态（防止无效提交）
            AddButton.IsEnabled = isActionSetValid && isActionNameValid;
        }

        /// <summary>
        /// 更新预览
        /// 显示按钮将如何显示
        /// </summary>
        private void UpdatePreview()
        {
            string actionName = ActionNameTextBox.Text.Trim();
            
            if (!string.IsNullOrEmpty(actionName))
            {
                // 使用和主窗口相同的截取逻辑
                string buttonText = GetSafeSubstring(actionName, 0, 2);
                PreviewText.Text = $"\"{buttonText}\" (完整名称: {actionName})";
                PreviewPanel.Visibility = Visibility.Visible;
            }
            else
            {
                PreviewPanel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 安全截取字符串
        /// C# 官方最佳实践：防止索引越界
        /// </summary>
        private string GetSafeSubstring(string text, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            
            if (startIndex >= text.Length)
                return string.Empty;
            
            int actualLength = Math.Min(length, text.Length - startIndex);
            return text.Substring(startIndex, actualLength);
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string actionSet = ActionSetTextBox.Text.Trim();
            string actionName = ActionNameTextBox.Text.Trim();

            // 最后验证（防御性编程）
            if (string.IsNullOrEmpty(actionSet) || string.IsNullOrEmpty(actionName))
            {
                NotificationWindow.Show("💡 提示", "请填写完整信息", 0.5);
                return;
            }

            ActionInfo = new PhotoshopActionInfo
            {
                ActionSetName = actionSet,
                ActionName = actionName
            };

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

