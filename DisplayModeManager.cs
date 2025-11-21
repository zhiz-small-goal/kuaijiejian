using System.IO;
using System.Text.Json;

namespace Kuaijiejian
{
    /// <summary>
    /// 按钮显示模式枚举（已简化为仅文字模式）
    /// </summary>
    public enum ButtonDisplayMode
    {
        /// <summary>显示文字（名称前2个字）</summary>
        Text
    }

    /// <summary>
    /// 显示模式管理器
    /// 管理窗口跟随设置和按钮布局的保存和加载
    /// </summary>
    public static class DisplayModeManager
    {
        private static readonly string ConfigPath = Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory,
            "display_mode_config.json"
        );

        private static bool _enableWindowFollow = true; // 默认启用窗口跟随
        private static int _buttonsPerRow = 5; // 默认每行5个按钮
        
        /// <summary>
        /// 最大允许的行数
        /// </summary>
        public const int MaxRows = 40;

        /// <summary>
        /// 当前显示模式（固定为文字模式）
        /// </summary>
        public static ButtonDisplayMode CurrentMode => ButtonDisplayMode.Text;

        /// <summary>
        /// 是否启用窗口跟随Photoshop
        /// </summary>
        public static bool EnableWindowFollow
        {
            get => _enableWindowFollow;
            set
            {
                _enableWindowFollow = value;
                SaveMode();
            }
        }

        /// <summary>
        /// 每行按钮数量（1-100）
        /// </summary>
        public static int ButtonsPerRow
        {
            get => _buttonsPerRow;
            set
            {
                // 限制范围：1-100个按钮
                if (value >= 1 && value <= 100)
                {
                    _buttonsPerRow = value;
                    SaveMode();
                }
            }
        }

        /// <summary>
        /// 加载窗口跟随设置和按钮布局配置
        /// </summary>
        public static void LoadMode()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<DisplayModeConfig>(json);
                    
                    if (config != null)
                    {
                        _enableWindowFollow = config.EnableWindowFollow;
                        
                        // 加载按钮数量配置，如果不存在则使用默认值5
                        if (config.ButtonsPerRow >= 1 && config.ButtonsPerRow <= 100)
                        {
                            _buttonsPerRow = config.ButtonsPerRow;
                        }
                        else
                        {
                            _buttonsPerRow = 5; // 默认值
                        }
                    }
                }
            }
            catch
            {
                // 加载失败，使用默认值
                _enableWindowFollow = true;
                _buttonsPerRow = 5;
            }
        }

        /// <summary>
        /// 保存窗口跟随设置和按钮布局配置
        /// </summary>
        private static void SaveMode()
        {
            try
            {
                var config = new DisplayModeConfig 
                { 
                    EnableWindowFollow = _enableWindowFollow,
                    ButtonsPerRow = _buttonsPerRow
                };
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // 保存失败，忽略
            }
        }

        private class DisplayModeConfig
        {
            public bool EnableWindowFollow { get; set; } = true; // 默认启用
            public int ButtonsPerRow { get; set; } = 5; // 默认每行5个按钮
        }
    }
}


