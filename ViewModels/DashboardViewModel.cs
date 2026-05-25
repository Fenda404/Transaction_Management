using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Models;
using Transaction_Management.Services;

namespace Transaction_Management.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        #region Properties
        TransactionService transactionService = new TransactionService();

        private Transactions _transactions;

        public List<Transactions> RecentTransactions { get; set; }

        private decimal _getTotalBalance;
        public decimal GetTotalBalance
        {
            get => _getTotalBalance;
            set
            {
                _getTotalBalance = value;
                OnPropertyChanged(nameof(GetTotalBalance));
            }
        }
        private decimal _getMonthlyIncome;
        public decimal GetMonthlyIncome
        {
            get => _getMonthlyIncome;
            set
            {
                _getMonthlyIncome = value;
                OnPropertyChanged(nameof(GetMonthlyIncome));
            }
        }
        private decimal _getMonthlyExpense;
        public decimal GetMonthlyExpense
        {
            get => _getMonthlyExpense;
            set
            {
                _getMonthlyExpense = value;
                OnPropertyChanged(nameof(GetMonthlyExpense));
            }
        }
        #endregion
        #region Constructor
        public DashboardViewModel()
        {
            TotalBalance();
            MonthlyIncome();
            MonthlyExpense();
            LoadMonthyTransactions();
        }
        #endregion
        #region Methods

        /// <summary>
        /// Hàm này để tính tổng số dư của người dùng hiện tại bằng cách lấy tất cả các giao dịch của họ từ TransactionService, áp dụng hàm GetSignedAmount để tính toán số tiền có dấu (dương hoặc âm) và sau đó tổng hợp chúng lại để có được số dư cuối cùng. 
        /// Kết quả được lưu vào property GetTotalBalance để hiển thị trên giao diện người dùng. 
        /// Việc sử dụng LINQ giúp mã ngắn gọn và dễ đọc hơn khi xử lý tập hợp dữ liệu.
        /// </summary>
        /// <returns></returns>
        private decimal TotalBalance()
        {
            GetTotalBalance = transactionService.Transactions
                            .Where(t => t.UserID == UserSessionService.CurrentUser.UserID)
                            .Sum(t => transactionService.GetSignedAmount(t));
            return GetTotalBalance;
        }

        /// <summary>
        /// Hàm này để tính tổng thu nhập trong tháng hiện tại của người dùng bằng cách lọc các giao dịch của họ từ TransactionService dựa trên UserID, 
        /// tháng và năm của ngày giao dịch.
        /// </summary>
        /// <returns></returns>
        private decimal MonthlyIncome()
        {
            GetMonthlyIncome = transactionService.Transactions
                            .Where(t => t.UserID == UserSessionService.CurrentUser.UserID
                                        && t.TransactionDate.HasValue
                                        && t.TransactionDate.Value.Month == DateTime.Now.Month
                                        && t.TransactionDate.Value.Year == DateTime.Now.Year)
                            .Sum(t => transactionService.GetSignedAmount(t) > 0 ? transactionService.GetSignedAmount(t) : 0);
            return GetMonthlyIncome;
        }

        /// <summary>
        /// Hàm này để tính tổng chi tiêu trong tháng hiện tại của người dùng bằng cách lọc các giao dịch của họ từ TransactionService dựa trên UserID,
        /// tháng và năm của ngày giao dịch.
        /// </summary>
        /// <returns></returns>
        private decimal MonthlyExpense()
        {
            GetMonthlyExpense = transactionService.Transactions
                            .Where(t => t.UserID == UserSessionService.CurrentUser.UserID
                                        && t.TransactionDate.HasValue
                                        && t.TransactionDate.Value.Month == DateTime.Now.Month
                                        && t.TransactionDate.Value.Year == DateTime.Now.Year)
                            .Sum(t => transactionService.GetSignedAmount(t) < 0 ? -transactionService.GetSignedAmount(t) : 0);
            return GetMonthlyExpense;
        }

        private void LoadMonthyTransactions()
        {
                RecentTransactions = transactionService.Transactions
                                .Where(t => t.UserID == UserSessionService.CurrentUser.UserID
                                            && t.TransactionDate.HasValue
                                            && t.TransactionDate.Value.Month == DateTime.Now.Month
                                            && t.TransactionDate.Value.Year == DateTime.Now.Year)
                                .ToList();
        }

        #endregion

    }
}
