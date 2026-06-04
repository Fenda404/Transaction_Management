using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Models;

namespace Transaction_Management.Services
{
    internal class TransactionService
    {
        private readonly TMDatabase _context = new TMDatabase();

        #region Read Operations (Get Data)

        public async Task<List<Transactions>> LoadDataByUserIdAsync(int userId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Categories)
                .Include(t => t.Wallets)
                .Include(t => t.Users)
                .Where(t => t.UserID == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<Categories>> GetCategoriesAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<List<Wallets>> GetWalletsAsync()
        {
            return await _context.Wallets.AsNoTracking().ToListAsync();
        }

        public async Task<List<Budgets>> GetBudgetsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Budgets
                    .AsNoTracking()
                    .Include(b => b.Categories)
                    .Where(b => b.UserID == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetBudgetsByUserIdAsync: {ex.Message}");
                return new List<Budgets>();
            }
        }

        /// <summary>
        /// Kiểm tra xem người dùng có danh mục nào bị xài vượt quá ngân sách đã đặt trong tháng này không
        /// </summary>
        /// <returns>Trả về chuỗi danh sách các danh mục bị vượt ngân sách để hiện thông báo, nếu không vượt trả về chuỗi rỗng</returns>
        public async Task<string> CheckBudgetViolationsAsync(int userId)
        {
            try
            {
                DateTime firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                // 1. Lấy tất cả ngân sách người dùng đã cấu hình
                var userBudgets = await _context.Budgets
                    .AsNoTracking()
                    .Include(b => b.Categories)
                    .Where(b => b.UserID == userId)
                    .ToListAsync();

                if (!userBudgets.Any()) return string.Empty;

                // 2. Lấy tất cả giao dịch "Chi" trong tháng này của người dùng
                var monthlyExpenses = await _context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserID == userId
                             && t.TransactionDate >= firstDayOfMonth
                             && t.Categories.CategoryType.ToLower() == "expense")
                    .ToListAsync();

                StringBuilder alertMessage = new StringBuilder();

                // 3. So sánh chi tiêu thực tế với hạn mức của từng ngân sách
                foreach (var budget in userBudgets)
                {
                    // Tính tổng tiền đã tiêu cho danh mục này
                    decimal totalSpent = monthlyExpenses
                        .Where(t => t.CategoryID == budget.CategoryID)
                        .Sum(t => t.Amount);

                    // Nếu chi tiêu vượt quá số tiền ngân sách cho phép (Amount của bảng Budgets)
                    if (totalSpent > budget.AmountLimit)
                    {
                        alertMessage.AppendLine($"- Danh mục {budget.Categories.CategoryName}: Đã tiêu {totalSpent:N0} / Hạn mức {budget.AmountLimit:N0} VNĐ");
                    }
                }

                return alertMessage.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi kiểm tra ngân sách: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion

        #region Write Operations (Add / Edit / Delete)

        public async Task<bool> AddTransactionAsync(decimal amount, string walletName, string categoryType, DateTime date, string description)
        {
            try
            {
                if (UserSessionService.CurrentUser == null) throw new Exception("Người dùng chưa đăng nhập.");

                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.WalletName == walletName)
                    ?? throw new Exception($"Ví '{walletName}' không tồn tại.");
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryType)
                    ?? throw new Exception($"Danh mục '{categoryType}' không tồn tại.");

                var transaction = new Transactions
                {
                    Amount = amount,
                    TransactionDate = date == DateTime.MinValue ? DateTime.Now : date,
                    Note = description,
                    WalletID = wallet.WalletID,
                    CategoryID = category.CategoryID,
                    UserID = UserSessionService.CurrentUser.UserID
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thêm giao dịch: {ex.Message}", ex);
            }
        }

        public async Task<bool> EditTransactionAsync(int transactionID, decimal amount, string walletName, string categoryType, DateTime date, string description)
        {
            try
            {
                if (UserSessionService.CurrentUser == null) throw new Exception("Người dùng chưa đăng nhập.");

                var existingTransaction = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionID == transactionID)
                    ?? throw new Exception($"Không tìm thấy giao dịch ID {transactionID}");
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.WalletName == walletName)
                    ?? throw new Exception($"Ví '{walletName}' không tồn tại.");
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryType)
                    ?? throw new Exception($"Danh mục '{categoryType}' không tồn tại.");

                existingTransaction.Amount = amount;
                existingTransaction.TransactionDate = date == DateTime.MinValue ? DateTime.Now : date;
                existingTransaction.Note = description;
                existingTransaction.WalletID = wallet.WalletID;
                existingTransaction.CategoryID = category.CategoryID;
                existingTransaction.UserID = UserSessionService.CurrentUser.UserID;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi sửa giao dịch: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteTransactionAsync(int transactionId)
        {
            try
            {
                var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionID == transactionId);
                if (transaction == null) return false;

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa giao dịch: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAllTransactionsAsync()
        {
            try
            {
                if (UserSessionService.CurrentUser == null) return false;
                int currentUserId = UserSessionService.CurrentUser.UserID;

                await _context.Database.ExecuteSqlCommandAsync("DELETE FROM Transactions WHERE UserID = @p0", currentUserId);
                _context.Transactions.Local.Clear();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa sạch giao dịch: {ex.Message}", ex);
            }
        }

        #endregion

        #region Helper Methods

        public decimal GetSignedAmount(Transactions transaction)
        {
            if (transaction.Categories == null) return transaction.Amount;
            return transaction.Categories.CategoryType.ToLower() == "income" ? transaction.Amount : -transaction.Amount;
        }
        /// <summary>
        /// Kiểm tra xem hôm nay người dùng đã nhập giao dịch nào chưa
        /// </summary>
        /// <returns>Trả về true nếu ĐÃ NHẬP, trả về false nếu CHƯA NHẬP gì cả</returns>
        public async Task<bool> IsTransactionEnteredTodayAsync(int userId)
        {
            try
            {
                // Lấy ngày hôm nay (00:00:00)
                DateTime today = DateTime.Today;

                // Đếm xem có giao dịch nào có ngày lớn hơn hoặc bằng 00:00 hôm nay không
                bool hasTransaction = await _context.Transactions
                    .AsNoTracking()
                    .AnyAsync(t => t.UserID == userId && t.TransactionDate >= today);

                return hasTransaction;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi check nhập liệu hàng ngày: {ex.Message}");
                return true; // Nếu lỗi thì trả về true để tránh làm phiền người dùng bắn thông báo lỗi
            }
        }

        /// <summary>
        /// Kiểm tra xem người dùng đã có giao dịch nào trong ngày hôm nay chưa
        /// </summary>
        public async Task<bool> CheckUserHasEnteredTransactionTodayAsync(int userId)
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    // Lấy ra ngày hôm nay (chỉ lấy phần Ngày/Tháng/Năm, bỏ qua Giờ/Phút/Giây)
                    DateTime today = DateTime.Today;

                    // Quét trong bảng Transactions xem có dòng nào thuộc UserId này và được tạo vào hôm nay không
                    bool hasTransactions = await context.Transactions.AnyAsync(t =>
                        t.UserID == userId &&
                        t.TransactionDate >= today);

                    return hasTransactions;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi kiểm tra lịch sử nhập liệu trong ngày: {ex.Message}");
                // Nếu lỗi DB, trả về true để tạm thời ẩn thông báo, tránh làm phiền người dùng liên tục
                return true;
            }
        }
        #endregion
    }
}