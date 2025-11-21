using System.ComponentModel;

namespace Kuaijiejian
{
    /// <summary>
    /// 功能项数据模型
    /// </summary>
    public class FunctionItem : INotifyPropertyChanged
    {
        private string _name = "";
        private string _icon = "";
        private string _hotkey = "";
        private string _command = "";
        private string? _customColor = null;

        /// <summary>
        /// 功能名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// 图标（Emoji）
        /// </summary>
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        /// <summary>
        /// 快捷键显示
        /// </summary>
        public string Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = value;
                OnPropertyChanged(nameof(Hotkey));
            }
        }

        /// <summary>
        /// 要执行的命令或程序路径
        /// </summary>
        public string Command
        {
            get => _command;
            set
            {
                _command = value;
                OnPropertyChanged(nameof(Command));
            }
        }

        /// <summary>
        /// 功能分类（System 或 Application）
        /// </summary>
        public string Category { get; set; } = "System";

        /// <summary>
        /// 功能类型（Normal=普通命令, PhotoshopScript=PS脚本, PhotoshopAction=PS动作）
        /// </summary>
        public string FunctionType { get; set; } = "Normal";

        /// <summary>
        /// Photoshop 动作集名称（当 FunctionType 为 PhotoshopAction 时使用）
        /// </summary>
        public string ActionSetName { get; set; } = "";

        /// <summary>
        /// Photoshop 动作名称（当 FunctionType 为 PhotoshopAction 时使用）
        /// </summary>
        public string ActionName { get; set; } = "";

        /// <summary>
        /// 自定义按钮颜色（Hex格式，如 #FF5733，null表示使用主题默认色）
        /// </summary>
        public string? CustomColor
        {
            get => _customColor;
            set
            {
                _customColor = value;
                OnPropertyChanged(nameof(CustomColor));
                OnPropertyChanged(nameof(HasCustomColor));
            }
        }

        /// <summary>
        /// 是否有自定义颜色
        /// </summary>
        public bool HasCustomColor => !string.IsNullOrEmpty(_customColor);

        /// <summary>
        /// 按钮显示文本（固定为文字模式：显示名称前2个字）
        /// </summary>
        public string DisplayText
        {
            get
            {
                // 文字模式：显示名称前2个字
                return GetSafeSubstring(Name, 0, 2);
            }
        }

        /// <summary>
        /// 安全截取字符串
        /// </summary>
        private string GetSafeSubstring(string text, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            
            if (startIndex >= text.Length)
                return string.Empty;
            
            int actualLength = System.Math.Min(length, text.Length - startIndex);
            return text.Substring(startIndex, actualLength);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        /// <summary>
        /// 刷新显示文本（当显示模式切换时调用）
        /// </summary>
        public void RefreshDisplayText()
        {
            OnPropertyChanged(nameof(DisplayText));
        }
    }
}


