using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Models;

namespace Transaction_Management.Services
{
    internal class AdminService
    {
        
        public ObservableCollection<Users> Users { get; set; }

        public async Task<List<Users>> LoadUsersIdAsync()
        {
            try
            {
                using (var db = new TMDatabase()) 
                {
                    return await db.Users.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi DB: {ex.Message}");
                return new List<Users>();
            }
        }

        public async Task<bool> AddUserAsync(Users newUser)
        {
            try
            {
                using (var db = new TMDatabase())
                {
                    // Kiểm tra xem Username đã tồn tại dưới DB chưa để tránh trùng lặp trùng khóa
                    bool isExist = await db.Users.AnyAsync(u => u.Username.ToLower() == newUser.Username.ToLower());
                    if (isExist)
                    {
                        System.Diagnostics.Debug.WriteLine("Tên đăng nhập này đã tồn tại.");
                        return false;
                    }

                    db.Users.Add(newUser);
                    await db.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi AddUserAsync: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> UpdateUserAsync(Users userToUpdate)
        {
            try
            {
                using (var db = new TMDatabase())
                {
                    // Tìm tài khoản gốc trong DB dựa trên ID
                    var existingUser = await db.Users.FindAsync(userToUpdate.UserID);
                    if (existingUser == null) return false;

                    // Tiến hành cập nhật các trường thông tin thay đổi từ Form
                    existingUser.Email = userToUpdate.Email;
                    existingUser.RoleID = userToUpdate.RoleID;

                    // Chỉ cập nhật mật khẩu nếu chuỗi nhập vào không phải chuỗi mã hóa giả lập "********"
                    if (!string.IsNullOrWhiteSpace(userToUpdate.PasswordHash) && userToUpdate.PasswordHash != "********")
                    {
                        existingUser.PasswordHash = userToUpdate.PasswordHash; // Nên băm mật khẩu (Hash) ở đây nếu dự án yêu cầu bảo mật
                    }

                    await db.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi UpdateUserAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            try
            {
                using (var db = new TMDatabase())
                {
                    var user = await db.Users.FindAsync(userId);
                    if (user == null) return (false, "Người dùng không tồn tại.");

                    
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();

                    return (true, "Xóa tài khoản và toàn bộ dữ liệu liên quan thành công.");
                }
            }
            catch (DbUpdateException ex)
            {
                string inner = ex.InnerException?.Message ?? ex.Message;
                return (false, $"Lỗi ràng buộc dữ liệu: {inner}. (Gợi ý: Cần bật Cascade Delete trong DB)");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }
    }
}
