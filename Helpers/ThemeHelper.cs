using System;
using System.Linq;
using System.Windows;

namespace Transaction_Management.Helpers
{
    public static class ThemeHelper
    {
        /// <summary>
        /// Thay đổi giao diện màu sắc toàn ứng dụng dựa vào mã màu tuần tự (0, 1, 2) và chế độ DarkMode
        /// </summary>
        /// <param name="themeCode">Mã màu truyền vào ("0" = Green, "1" = Blue, "2" = Red)</param>
        /// <param name="isDarkMode">Trạng thái DarkMode (Mặc định là false - Light)</param>
        public static void ChangeTheme(string themeCode, bool isDarkMode = false)
        {
            // 1. Ánh xạ chuẩn theo bộ số 0, 1, 2
            string colorName = "Green";
            switch (themeCode)
            {
                case "0":
                case "Green":
                case "DarkGreen": // Hỗ trợ tương thích nếu DB cũ còn chuỗi chữ
                    colorName = "Green";
                    break;
                case "1":
                case "Blue":
                    colorName = "Blue";
                    break;
                case "2":
                case "Red":
                    colorName = "Red";
                    break;
                default:
                    colorName = "Green"; // Dự phòng mặc định
                    break;
            }

            // 2. Xác định tên file dựa trên chế độ sáng/tối
            string mode = isDarkMode ? "Dark" : "Light";
            string themeFile = $"{colorName}{mode}Theme.xaml";

            // Nếu bạn giữ nguyên cách đặt tên file cũ của màu Xanh lá (GreenTheme / DarkGreenTheme)
            if (colorName == "Green")
            {
                themeFile = isDarkMode ? "DarkGreenTheme.xaml" : "GreenTheme.xaml";
            }

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            // 3. Quét ngược danh sách gỡ bỏ sạch sẽ các file đuôi "Theme.xaml" cũ để tránh bị đè đống, xung đột màu
            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                if (dictionaries[i].Source?.ToString().Contains("Theme.xaml") == true)
                {
                    dictionaries.RemoveAt(i);
                }
            }

            // 4. Nhúng file giao diện màu mới vào hệ thống ứng dụng công bằng
            try
            {
                var newTheme = new ResourceDictionary
                {
                    Source = new Uri($"/Transaction_Management;component/Styles/Main/{themeFile}", UriKind.Relative)
                };

                // Add vào cuối danh sách Merged để WPF ưu tiên nạp đè cấu hình màu mới lên giao diện
                dictionaries.Add(newTheme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Không tìm thấy hoặc lỗi cấu trúc file Resource {themeFile}: {ex.Message}");
            }
        }
    }
}