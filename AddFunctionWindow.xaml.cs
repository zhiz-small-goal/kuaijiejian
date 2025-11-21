using System.Windows;
using Microsoft.Win32;

namespace Kuaijiejian
{
    public partial class AddFunctionWindow : Window
    {
        public FunctionItem? NewFunction { get; private set; }
        public string Category { get; set; } = "System";

        public AddFunctionWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证输入 - 只需要名称即可
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                NotificationWindow.Show("💡 提示", "请输入功能名称", 0.5);
                return;
            }

            // 创建新功能
            NewFunction = new FunctionItem
            {
                Name = NameTextBox.Text.Trim(),
                Icon = string.IsNullOrWhiteSpace(IconTextBox.Text) ? "📌" : IconTextBox.Text.Trim(),
                Hotkey = HotkeyTextBox.Text.Trim(),
                Command = string.IsNullOrWhiteSpace(CommandTextBox.Text) ? "notepad.exe" : CommandTextBox.Text.Trim(),
                Category = this.Category
            };

            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*",
                Title = "选择程序"
            };

            if (dialog.ShowDialog() == true)
            {
                CommandTextBox.Text = dialog.FileName;
            }
        }
    }
}

