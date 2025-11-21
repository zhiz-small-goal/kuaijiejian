using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Kuaijiejian
{
    /// <summary>
    /// Hex颜色字符串转Color对象的转换器
    /// </summary>
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(hexColor);
                }
                catch
                {
                    // 如果转换失败，返回透明色
                    return Colors.Transparent;
                }
            }
            
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            
            return string.Empty;
        }
    }
}





