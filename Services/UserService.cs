using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Helpers;
using Transaction_Management.Models;

namespace Transaction_Management.Services
{
    internal class UserService
    {
        /// <summary>
        /// Tải thông tin Fullname và Email của người dùng dựa vào Username.
        /// </summary>
        public async Task<(int Id, string Fullname, string Email, string Currency)?> LoadUserInfoAsync(string username)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);

                    if (user != null)
                    {
                        return (
                            user.UserID,
                            user.FullName,
                            user.Email,
                            user.Currency ?? "VNĐ"
                        );
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadUserInfoAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Thay đổi Username của người dùng hiện tại
        /// </summary>
        public async Task<bool> ChangeUsernameAsync(string currentUsername, string newUsername)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    bool isExist = await context.Users.AnyAsync(u => u.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase));
                    if (isExist) return false;

                    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);
                    if (user == null) return false;

                    user.Username = newUsername;
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đổi Username: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Thay đổi mật khẩu của người dùng hiện tại
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string username, string newPassword)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                    if (user == null) return false;

                    user.PasswordHash = newPassword;
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đổi mật khẩu: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cập nhật Email của người dùng
        /// </summary>
        public async Task<bool> UpdateEmailAsync(string username, string newEmail)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                    if (user == null) return false;

                    user.Email = newEmail;
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật Email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cập nhật Họ và tên của người dùng
        /// </summary>
        public async Task<bool> UpdateFullNameAsync(string username, string newFullName)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                    if (user == null) return false;

                    user.FullName = newFullName;
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật FullName: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cập nhật Loại tiền tệ dựa vào UserId (Đã lược bỏ đoạn gán nhầm cấu hình thông báo)
        /// </summary>
        public async Task<bool> UpdateCurrencyByUserIdAsync(int userId, string currency)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                    if (user != null)
                    {
                        user.Currency = currency;

                        context.Entry(user).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nghiêm trọng UpdateCurrencyByUserIdAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Nạp nhanh cấu hình Tiền tệ và Định dạng số vào AppConfig ngay khi đăng nhập thành công
        /// </summary>
        public async Task LoadUserConfigToAppConfigAsync(string username)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                    if (user != null)
                    {
                        AppConfig.CurrentCurrency = !string.IsNullOrEmpty(user.Currency) ? user.Currency : "VNĐ";
                        AppConfig.RaiseCurrencyChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp cấu hình sớm: {ex.Message}");
            }
        }

        /// <summary>
        /// CHỨC NĂNG MỚI: Cập nhật cấu hình bật/tắt thông báo của User xuống SQL Server
        /// </summary>
        public async Task<bool> UpdateUserSettingsAsync(int userId, bool isBudgetAlert, bool isDailyReminder)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                    if (user != null)
                    {
                        // Gán giá trị nhận được từ tham số vào thuộc tính thực thể
                        user.IsBudgetAlert = isBudgetAlert;
                        user.IsDailyReminder = isDailyReminder;

                        context.Entry(user).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật cấu hình thông báo User: {ex.Message}");
                return false;
            }
        }

        // <summary>
        /// CHỨC NĂNG MỚI: Thực hiện ghi nhận trạng thái màu sắc ứng dụng dựa trên UserId công khai
        /// </summary>
        public async Task<bool> UpdateThemeColorAsync(int userId, string themeName)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                    if (user != null)
                    {
                        user.ThemeColor = themeName;

                        context.Entry(user).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu cấu hình Theme xuống Database: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Cập nhật trạng thái Dark Mode của người dùng dựa trên chuỗi mã ("0" = false, "1" = true)
        /// </summary>
        public async Task<bool> UpdateDarkModeAsync(int userId, string darkModeCode)
        {
            try
            {
                // Ánh xạ quy ước: "1" là true (Bật Dark Mode), còn lại ("0" hoặc bất kỳ gì khác) là false
                bool isDarkModeValue = (darkModeCode == "1");

                using (var db = new TMDatabase())
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                    if (user != null)
                    {
                        user.IsDarkMode = isDarkModeValue; // Gán giá trị bool vào thuộc tính ứng với cột BIT

                        await db.SaveChangesAsync();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Lỗi khi cập nhật DarkMode trong UserService: {ex.Message}");
                return false;
            }
        }
    }

}