using System.Globalization;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FMAuditTester
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status.Contains("200 OK"))
                {
                    return Brushes.Green;
                }
                else if (status.StartsWith("Error"))
                {
                    return Brushes.Red;
                }
                else if (status.Contains("Resolved"))
                {
                    return Brushes.Green;
                }
                else if (status.Contains("Not resolved"))
                {
                    return Brushes.Red;
                }
                else
                {
                    return Brushes.Orange;
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}