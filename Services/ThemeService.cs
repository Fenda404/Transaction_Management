using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Transaction_Management.Models;

namespace Transaction_Management.Services
{
    public static class ThemeService
    {
        /// <summary>
        /// Lấy đồng thời mã màu Theme và trạng thái Dark Mode của người dùng từ Database
        /// </summary>
        /// <param name="userId">ID của người dùng cần tra cứu</param>
        /// <returns>Trả về một Tuple gồm (ThemeColor, IsDarkMode)</returns>
        public static async Task<(string ThemeColor, bool IsDarkMode)> GetThemeForUser(int userId)
        {
            try
            {
                using (var db = new TMDatabase())
                {
                    // Truy vấn đồng thời cả 2 cột ThemeColor và IsDarkMode thành một Object tạm (Anonymous Type)
                    var userThemeData = await db.Users
                        .Where(u => u.UserID == userId)
                        .Select(u => new
                        {
                            u.ThemeColor,
                            u.IsDarkMode
                        })
                        .FirstOrDefaultAsync();

                    if (userThemeData != null)
                    {
                        // Nếu cột ThemeColor trống, mặc định lấy mã "0" (Xanh lá)
                        string color = !string.IsNullOrEmpty(userThemeData.ThemeColor) ? userThemeData.ThemeColor : "0";
                        return (color, userThemeData.IsDarkMode);
                    }

                    // Trường hợp tìm không thấy User này trong DB, trả về cấu hình mặc định (Gốc: "0", LightMode: false)
                    return ("0", false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Lỗi truy vấn Theme & DarkMode từ DB: {ex.Message}");
                return ("0", false); // Dự phòng an toàn khi sập hoặc lỗi kết nối DB
            }
        }
    }
}