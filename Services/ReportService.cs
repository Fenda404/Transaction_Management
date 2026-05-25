using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Models;
using Transaction_Management.ViewModels;
using System.Data.Entity;
namespace Transaction_Management.Services
{
    internal class ReportService
    {
        public TM_Database db_transactions = new TM_Database();
        public ObservableCollection<Transactions> Transactions { get; set; }
        public ObservableCollection<Categories> Categories { get; set; }

        public ObservableCollection<Wallets> Wallets { get; set; }
        private TransactionService _transactionService = new TransactionService();

        public ReportService()
        {

        }
        /// <summary>
        /// Lấy dữ liệu báo cáo cho một người dùng cụ thể.
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Tuple chứa tổng thu, tổng chi, và danh sách chi tiết từng hạng mục chi</returns>
        public (decimal TotalIncome, decimal TotalExpense, List<CategoryBreakdownItem> CategoryBreakdown)
           GetReportData(int userId)
        {
            var userTransactions = db_transactions.Transactions
                .Include(t => t.Categories)
                .Where(t => t.UserID == userId)
                .ToList();

            decimal totalIncome = userTransactions
                .Where(t => t.Categories != null &&
                            t.Categories.CategoryType.Equals("income", StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Amount);

            decimal totalExpense = userTransactions
                .Where(t => t.Categories != null &&
                            t.Categories.CategoryType.Equals("expense", StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Amount);

            var expenseGroups = userTransactions
                .Where(t => t.Categories != null &&
                            t.Categories.CategoryType.Equals("expense", StringComparison.OrdinalIgnoreCase))
                .GroupBy(t => new { t.CategoryID, t.Categories.CategoryName })
                .Select(g => new
                {
                    g.Key.CategoryName,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToList();

            var categoryBreakdown = new List<CategoryBreakdownItem>();
            foreach (var item in expenseGroups)
            {
                double percentage = totalExpense > 0 ? (double)(item.TotalAmount / totalExpense) * 100 : 0;
                categoryBreakdown.Add(new CategoryBreakdownItem
                {
                    CategoryName = item.CategoryName,
                    Amount = item.TotalAmount,
                    Percentage = Math.Round(percentage, 1),
                    ColorCode = GetColorForCategory(item.CategoryName)
                });
            }

            return (totalIncome, totalExpense, categoryBreakdown);
        }

        /// <summary>
        /// Gán màu sắc cho từng danh mục (có thể tùy chỉnh hoặc lấy từ DB)
        /// </summary>
        private string GetColorForCategory(string categoryName)
        {
            var colors = new[] { "#4CAF50", "#FF9800", "#9C27B0", "#2196F3", "#E91E63", "#00BCD4", "#FF5722", "#8BC34A", "#673AB7", "#FFC107" };
            int index = Math.Abs(categoryName.GetHashCode()) % colors.Length;
            return colors[index];
        }
    }
}
