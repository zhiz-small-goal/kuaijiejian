using System.Windows;

namespace Kuaijiejian
{
    public partial class InputDialog : Window
    {
        public string InputText { get; private set; } = "";

        public InputDialog(string title, string defaultText = "")
        {
            InitializeComponent();
            
            this.Title = title;
            InputTextBox.Text = defaultText;
            
            // 移除对话框的 WS_EX_NOACTIVATE 标志，让它可以正常激活
            // 这不会影响主窗口的 no-activate 行为
            this.SourceInitialized += (s, e) =>
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                int exStyle = WindowsApiHelper.GetWindowLong(helper.Handle, WindowsApiHelper.GWL_EXSTYLE);
                // 移除 WS_EX_NOACTIVATE 标志（如果存在）
                WindowsApiHelper.SetWindowLong(helper.Handle, WindowsApiHelper.GWL_EXSTYLE, 
                    exStyle & ~WindowsApiHelper.WS_EX_NOACTIVATE);
            };
            
            // 窗口加载后激活并聚焦输入框
            this.Loaded += (s, e) =>
            {
                this.Activate();
                
                // 使用 Dispatcher 确保窗口完全激活后再聚焦
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    InputTextBox.Focus();
                    InputTextBox.SelectAll();
                    System.Windows.Input.Keyboard.Focus(InputTextBox);
                }), System.Windows.Threading.DispatcherPriority.Input);
            };
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(InputText))
            {
                NotificationWindow.Show("💡 提示", "名称不能为空", 0.5);
                return;
            }
            
            this.DialogResult = true;
            this.Close();
            
            // 返回焦点到 Photoshop
            WindowsApiHelper.ActivatePhotoshopWindow();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
            
            // 返回焦点到 Photoshop
            WindowsApiHelper.ActivatePhotoshopWindow();
        }
    }
}


