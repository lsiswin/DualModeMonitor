using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using MonitorLibrary.Models.Enums;

namespace DualModeMonitorSystem.Convert
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlertStatus status)
            {
                switch (status)
                {
                    case AlertStatus.New:
                        return new SolidColorBrush(Color.FromRgb(211, 56, 44)); // 红                   
                    case AlertStatus.Closed:
                        return new SolidColorBrush(Colors.Gray); // 灰色      
                    case AlertStatus.Acknowledged:
                        return new SolidColorBrush(Colors.Gray); // 灰色   
                    default:
                        return new SolidColorBrush(Colors.Gray); // 灰色   
                }
            }
            if (value is DeviceStatus deviceStatus)
            {
                switch (deviceStatus)
                {
                    case DeviceStatus.Error:
                        return new SolidColorBrush(Color.FromRgb(211, 56, 44)); // 红                   
                    case DeviceStatus.Normal:
                        return new SolidColorBrush(Colors.Green); // 绿色      
                    case DeviceStatus.Warning:
                        return new SolidColorBrush(Colors.Yellow); // 黄色
                    default:
                        return new SolidColorBrush(Colors.Gray); // 灰色   
                }
            }
            return new SolidColorBrush(Colors.Gray); // 灰色   
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
