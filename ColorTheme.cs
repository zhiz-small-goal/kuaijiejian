using System.Collections.Generic;
using System.Windows.Media;

namespace Kuaijiejian
{
    /// <summary>
    /// 配色主题类 - 参考Material Design与iOS设计规范
    /// </summary>
    public class ColorTheme
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        
        // 窗口背景
        public string WindowBackground { get; set; } = "#FFFFFF";
        
        // 标题栏渐变
        public string TitleBarStart { get; set; } = "#6b7280";
        public string TitleBarEnd { get; set; } = "#4b5563";
        
        // 左侧区域渐变
        public string LeftAreaStart { get; set; } = "#e5e7eb";
        public string LeftAreaEnd { get; set; } = "#d1d5db";
        
        // 右侧区域渐变
        public string RightAreaStart { get; set; } = "#d1d5db";
        public string RightAreaEnd { get; set; } = "#9ca3af";
        
        // 区域标题文字颜色
        public string SectionTitleColor { get; set; } = "#374151";
        
        // 状态栏背景
        public string StatusBarBackground { get; set; } = "#f3f4f6";
        
        // 状态栏文字颜色
        public string StatusBarForeground { get; set; } = "#4b5563";
        
        // === 按钮配色（参考专业设计规范） ===
        
        // 功能按钮背景色
        public string ButtonBackground { get; set; } = "#FFFFFF";
        
        // 功能按钮边框色
        public string ButtonBorder { get; set; } = "#E5E7EB";
        
        // 功能按钮悬停背景色
        public string ButtonHoverBackground { get; set; } = "#F9FAFB";
        
        // 功能按钮按下背景色
        public string ButtonActiveBackground { get; set; } = "#F3F4F6";
        
        // 功能按钮文字颜色
        public string ButtonTextColor { get; set; } = "#374151";
        
        // 功能按钮图标颜色
        public string ButtonIconColor { get; set; } = "#6B7280";
        
        // 功能按钮副文字颜色（快捷键）
        public string ButtonSubTextColor { get; set; } = "#9CA3AF";
        
        // === 添加按钮配色 ===
        
        // 添加按钮背景色
        public string AddButtonBackground { get; set; } = "#FFFFFF";
        
        // 添加按钮边框色
        public string AddButtonBorder { get; set; } = "#E5E7EB";
        
        // 添加按钮悬停背景色
        public string AddButtonHoverBackground { get; set; } = "#F9FAFB";
        
        // 添加按钮图标颜色
        public string AddButtonIconColor { get; set; } = "#6366F1";
        
        // === 焦点框配色 ===
        
        // 焦点框颜色
        public string FocusColor { get; set; } = "#6366F1";

        /// <summary>
        /// 获取所有预设主题
        /// 注意：其他58个主题已备份到 ColorTheme.Backup.cs
        /// </summary>
        public static List<ColorTheme> GetAllThemes()
        {
            return new List<ColorTheme>
            {
                // 极简单色 - 参考Apple系统设计
                new ColorTheme
                {
                    Name = "极简单色",
                    Description = "极致简约，让功能按钮成为视觉焦点",
                    WindowBackground = "#FFFFFF",
                    TitleBarStart = "#6b7280",
                    TitleBarEnd = "#4b5563",
                    LeftAreaStart = "#F8F9FA",
                    LeftAreaEnd = "#F1F3F4",
                    RightAreaStart = "#F1F3F4",
                    RightAreaEnd = "#E8EAED",
                    SectionTitleColor = "#202124",
                    StatusBarBackground = "#F8F9FA",
                    StatusBarForeground = "#5F6368",
                    
                    // 功能按钮 - 清爽简约，淡灰背景
                    ButtonBackground = "#F8F9FA",
                    ButtonBorder = "#DADCE0",
                    ButtonHoverBackground = "#E8EAED",
                    ButtonActiveBackground = "#E8F0FE",
                    ButtonTextColor = "#3C4043",
                    ButtonIconColor = "#5F6368",
                    ButtonSubTextColor = "#80868B",
                    
                    // 添加按钮 - 使用蓝色强调
                    AddButtonBackground = "#F8F9FA",
                    AddButtonBorder = "#4285F4",
                    AddButtonHoverBackground = "#E8F0FE",
                    AddButtonIconColor = "#4285F4",
                    
                    // 焦点框
                    FocusColor = "#4285F4"
                },
                
                // 粉色泡泡 - 参考Instagram/Pinterest设计
                new ColorTheme
                {
                    Name = "粉色泡泡",
                    Description = "梦幻泡泡，纯真烂漫",
                    WindowBackground = "#FDF7F0",
                    TitleBarStart = "#E91E63",
                    TitleBarEnd = "#F06292",
                    LeftAreaStart = "#FCE4EC",
                    LeftAreaEnd = "#F8BBD9",
                    RightAreaStart = "#F8BBD9",
                    RightAreaEnd = "#F48FB1",
                    SectionTitleColor = "#880E4F",
                    StatusBarBackground = "#FCE4EC",
                    StatusBarForeground = "#AD1457",
                    
                    // 功能按钮 - 温馨粉色系，淡粉背景
                    ButtonBackground = "#FCE4EC",
                    ButtonBorder = "#F8BBD9",
                    ButtonHoverBackground = "#F8BBD9",
                    ButtonActiveBackground = "#F48FB1",
                    ButtonTextColor = "#880E4F",
                    ButtonIconColor = "#C2185B",
                    ButtonSubTextColor = "#E91E63",
                    
                    // 添加按钮 - 使用深粉色强调
                    AddButtonBackground = "#FCE4EC", 
                    AddButtonBorder = "#E91E63",
                    AddButtonHoverBackground = "#F8BBD9",
                    AddButtonIconColor = "#E91E63",
                    
                    // 焦点框
                    FocusColor = "#E91E63"
                },
                
                // 温柔甜宠 - 参考星河幻季配色
                new ColorTheme
                {
                    Name = "温柔甜宠",
                    Description = "粉色系主导，温柔治愈配色",
                    WindowBackground = "#FFF8FA",
                    TitleBarStart = "#FFB2C3",
                    TitleBarEnd = "#FFDAE2",
                    LeftAreaStart = "#FFDAE2",
                    LeftAreaEnd = "#FFB2C3",
                    RightAreaStart = "#FFB2C3", 
                    RightAreaEnd = "#FF9BAD",
                    SectionTitleColor = "#8B4B6B",
                    StatusBarBackground = "#FFDAE2",
                    StatusBarForeground = "#A0566C",
                    
                    // 功能按钮 - 粉色系主导，樱花粉背景
                    ButtonBackground = "#FFDAE2",
                    ButtonBorder = "#FFB2C3",
                    ButtonHoverBackground = "#FFB2C3",
                    ButtonActiveBackground = "#FF9BAD",
                    ButtonTextColor = "#8B4B6B",
                    ButtonIconColor = "#C2185B",
                    ButtonSubTextColor = "#E91E63",
                    
                    // 添加按钮 - 甜心粉强调，黄色点缀
                    AddButtonBackground = "#FFDAE2",
                    AddButtonBorder = "#FFB2C3",
                    AddButtonHoverBackground = "#FFFEBC",
                    AddButtonIconColor = "#E91E63",
                    
                    // 焦点框
                    FocusColor = "#FFB2C3"
                },
                
                // 粉黛杏花 - 纯色粉色系
                new ColorTheme
                {
                    Name = "粉黛杏花",
                    Description = "纯色粉色系，清新淡雅",
                    WindowBackground = "#FFFBFD",
                    TitleBarStart = "#F3BCC8",
                    TitleBarEnd = "#F3BCC8",
                    LeftAreaStart = "#FBD2E3",
                    LeftAreaEnd = "#FBD2E3",
                    RightAreaStart = "#FBA4C6", 
                    RightAreaEnd = "#FBA4C6",
                    SectionTitleColor = "#9B4A6B",
                    StatusBarBackground = "#FDC4CE",
                    StatusBarForeground = "#9B4A6B",
                    
                    // 功能按钮 - 粉黛色
                    ButtonBackground = "#FDC4CE",
                    ButtonBorder = "#F3BCC8",
                    ButtonHoverBackground = "#FBA4C6",
                    ButtonActiveBackground = "#F3BCC8",
                    ButtonTextColor = "#9B4A6B",
                    ButtonIconColor = "#C2185B",
                    ButtonSubTextColor = "#E91E63",
                    
                    // 添加按钮 - 盈盈粉
                    AddButtonBackground = "#FDC4CE",
                    AddButtonBorder = "#FBA4C6",
                    AddButtonHoverBackground = "#F3BCC8",
                    AddButtonIconColor = "#E91E63",
                    
                    // 焦点框
                    FocusColor = "#FBA4C6"
                }
            };
        }
    }
}
