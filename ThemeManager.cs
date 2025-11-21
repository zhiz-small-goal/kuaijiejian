using System.IO;
using System.Linq;
using System.Text.Json;

namespace Kuaijiejian
{
    /// <summary>
    /// 主题管理器
    /// </summary>
    public static class ThemeManager
    {
        // 主题配置文件保存在应用程序目录（确保有写入权限）
        private static readonly string ConfigPath = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory,
            "theme_config.json"
        );
        private static ColorTheme? _currentTheme;

        /// <summary>
        /// 获取当前主题
        /// </summary>
        public static ColorTheme CurrentTheme
        {
            get
            {
                if (_currentTheme == null)
                {
                    LoadTheme();
                }
                return _currentTheme!;
            }
        }

        /// <summary>
        /// 加载保存的主题配置
        /// </summary>
        public static void LoadTheme()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<ThemeConfig>(json);
                    
                    if (config != null && !string.IsNullOrEmpty(config.ThemeName))
                    {
                        var theme = ColorTheme.GetAllThemes()
                            .FirstOrDefault(t => t.Name == config.ThemeName);
                        
                        if (theme != null)
                        {
                            _currentTheme = theme;
                            return;
                        }
                    }
                }
            }
            catch
            {
                // 加载失败，使用默认主题
            }

            // 默认使用极简单色
            _currentTheme = ColorTheme.GetAllThemes()[0];
        }

        /// <summary>
        /// 保存主题配置
        /// </summary>
        public static void SaveTheme(string themeName)
        {
            try
            {
                var config = new ThemeConfig { ThemeName = themeName };
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

        /// <summary>
        /// 应用主题
        /// </summary>
        public static void ApplyTheme(ColorTheme theme)
        {
            _currentTheme = theme;
            SaveTheme(theme.Name);
        }

        private class ThemeConfig
        {
            public string ThemeName { get; set; } = "";
        }
    }
}

