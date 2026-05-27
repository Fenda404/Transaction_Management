using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Transaction_Management.Converters
{
    internal class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            double doubleValue = System.Convert.ToDouble(value);
            double doubleParameter = System.Convert.ToDouble(parameter);

            return doubleValue > doubleParameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
