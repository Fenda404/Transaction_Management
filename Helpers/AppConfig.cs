using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Management.Helpers
{
    internal class AppConfig
    {
        // 1. Biến lưu ký hiệu tiền tệ hiện tại (Mặc định là đ)
        public static string CurrentCurrency { get; set; } = "VNĐ";

        // 2. Sự kiện dùng để phát tín hiệu "Cài đặt đã thay đổi!"
        public static event Action CurrencyChanged;
        public static bool IsBudgetAlert { get; set; } = true;
        public static bool IsDailyReminder { get; set; } = true;
        // 3. Hàm kích hoạt sự kiện trên
        public static void RaiseCurrencyChanged()
        {
            CurrencyChanged?.Invoke();
        }
    }
}
