using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Models;

namespace Transaction_Management.Services
{
    internal class BudgetService
    {
        private readonly TMDatabase db_Budgets = new TMDatabase();

        public ObservableCollection<Budgets> Budgets { get; set; }

        public ObservableCollection<Categories> Categories { get; set; }

        public ObservableCollection<Users> Users { get; set; }

        public async Task<List<Budgets>> LoadDataByUserIdAsync(int userId)
        {
            
            return await db_Budgets.Budgets
                .Include(b => b.Categories)
                .Include(b => b.Users)
                .Where(t => t.UserID == userId)
                .OrderByDescending(t => t.AmountLimit)
                .ToListAsync();
        }

        // Hỗ trợ lấy danh sách danh mục loại Chi tiêu (Expense) để hiển thị lên ComboBox khi tạo Ngân sách
        public async Task<List<Categories>> GetCategoriesAsync()
        {
            try
            {
                using (var db = new TMDatabase())
                {
                    return await db.Categories
                        .Where(c => c.CategoryType == "Expense")
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetCategoriesAsync: {ex.Message}");
                return new List<Categories>();
            }
        }

        // 2. CREATE: Thêm mới một ngân sách vào Database
        public async Task<bool> AddAsync(Budgets budget)
        {
            try
            {
                using (var db = new TMDatabase())
                {
                    db.Budgets.Add(budget);
                    await db.SaveChangesAsync();

                    // ĐỒNG BỘ RAM: Ép EF nạp thông tin Hạng mục (Categories) dựa theo CategoryID vừa thêm
                    // giúp ViewModel lấy được ngay tên hiển thị (CategoryName) mà không bị null
                    await db.Entry(budget).Reference(b => b.Categories).LoadAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi AddAsync: {ex.Message}");
                return false;
            }
        }

        // 3. UPDATE: Cập nhật hạn mức hoặc danh mục của ngân sách đã tồn tại
        public async Task<bool> UpdateAsync(Budgets budget)
        {
            try
            {
                using (var db = new TMDatabase())
                {
                    // Tìm thực thể gốc trong DB
                    var existingBudget = await db.Budgets.FindAsync(budget.BudgetID);
                    if (existingBudget == null) return false;

                    // Cập nhật các trường cho phép sửa
                    existingBudget.CategoryID = budget.CategoryID;
                    existingBudget.AmountLimit = budget.AmountLimit;

                    await db.SaveChangesAsync();

                    // ĐỒNG BỘ RAM: Sau khi lưu thay đổi ID xuống SQL, ép nạp lại thông tin danh mục mới 
                    // vào thực thể hiện tại để trả ngược ra giao diện
                    await db.Entry(existingBudget).Reference(b => b.Categories).LoadAsync();

                    // Gán thực thể đã nạp đầy đủ tên chữ lại cho đối tượng của ViewModel
                    budget.Categories = existingBudget.Categories;

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi UpdateAsync: {ex.Message}");
                return false;
            }
        }

        // 4. DELETE: Xóa một ngân sách khỏi hệ thống
        public async Task<bool> DeleteAsync(int budgetId)
        {
            try
            {
                using (var db = new TMDatabase())
                {
                    var budget = await db.Budgets.FindAsync(budgetId);
                    if (budget == null) return false;

                    db.Budgets.Remove(budget);
                    await db.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi DeleteAsync: {ex.Message}");
                return false;
            }
        }
    }
}
