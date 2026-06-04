using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Transaction_Management.Helpers;

namespace Transaction_Management.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                // Lấy định dạng từ AppConfig (Ví dụ nếu có lưu Format trong AppConfig)
                // Chuẩn hóa dấu phân cách theo cấu hình format đã chọn
                string formattedNumber = amount.ToString("N0", CultureInfo.InvariantCulture);

                // Khắc phục: Đọc trực tiếp từ AppConfig tại thời điểm UI yêu cầu render
                string currencySign = AppConfig.CurrentCurrency ?? "VNĐ";

                return $"{formattedNumber} {currencySign}";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
