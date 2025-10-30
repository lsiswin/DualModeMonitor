using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DualModeMonitorSystem.Convert
{
    /// <summary>
    /// 警告级别到颜色的转换器
    /// </summary>
    class AlertLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlertLevel level)
            {
                switch (level)
                {
                    case AlertLevel.Normal:
                        return new SolidColorBrush(Color.FromRgb(128, 255, 0)); // 绿色
                    case AlertLevel.Warning:
                        return new SolidColorBrush(Color.FromRgb(250, 173, 20)); // 黄色
                    case AlertLevel.Error:
                        return new SolidColorBrush(Color.FromRgb(255, 77, 79)); // 红色;
                    case AlertLevel.Offline:
                        return new SolidColorBrush(Color.FromRgb(134, 144, 156)); // 灰色
                    default:
                        return new SolidColorBrush(Colors.Gray); 

                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
