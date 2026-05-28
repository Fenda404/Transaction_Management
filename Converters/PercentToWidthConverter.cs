using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Transaction_Management.Converters
{
    internal class PercentToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage && parameter is double containerWidth)
            {
                // Giới hạn width không vượt quá containerWidth
                double width = (percentage / 100) * containerWidth;
                if (width > containerWidth) width = containerWidth;
                if (width < 0) width = 0;
                return width;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
