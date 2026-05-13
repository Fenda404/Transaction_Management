using System;
using System.Windows.Data;

namespace Transaction_Management.Converters
{
    public class PasswordVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = (bool)value;
            
            
            if (parameter?.ToString() == "icon")
            {
                
                return isVisible ? "\uE1A3" : "\uE1A4";
            }
            
            return isVisible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
