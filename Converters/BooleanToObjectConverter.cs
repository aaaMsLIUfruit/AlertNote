using System;
using System.Globalization;
using System.Windows.Data;

namespace StickyAlerts
{
    public class BooleanToObjectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string stringValue)
            {
                var values = stringValue.Split('|');
                if (values.Length == 2)
                {
                    return boolValue ? values[0] : values[1];
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 