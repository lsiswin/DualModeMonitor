using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DualModeMonitorSystem.Convert
{
    public class AlertStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlertStatus status)
            {
                switch (status)
                {
                    case AlertStatus.Unhandled:
                        return new SolidColorBrush(Color.FromRgb(211,56,44)); // 红                   
                    case AlertStatus.Handled:
                        return new SolidColorBrush(Colors.Gray); // 灰色      
                    case AlertStatus.Retry:
                        return new SolidColorBrush(Colors.Gray); // 灰色   
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
