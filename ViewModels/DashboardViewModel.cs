using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Transaction_Management.Models;
using Transaction_Management.Services;

namespace Transaction_Management.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        #region Properties
        private readonly TransactionService _transactionService = new TransactionService();

        // Danh sách hiển thị trực tiếp lên bảng "GIAO DỊCH GẦN ĐÂY TRONG THÁNG NÀY"
        private ObservableCollection<Transactions> _recentTransactions = new ObservableCollection<Transactions>();
        public ObservableCollection<Transactions> RecentTransactions
        {
            get => _recentTransactions;
            set
            {
                _recentTransactions = value;
                OnPropertyChanged(nameof(RecentTransactions));
            }
        }

        // 3 thuộc tính tương ứng với 3 thẻ chỉ số ở hàng trên cùng của UI
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
            // Kích hoạt tiến trình nạp và tính toán dữ liệu một cách an toàn
            _ = InitializeDashboardAsync();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Hàm khởi tạo dữ liệu Dashboard gọn nhẹ theo đúng giao diện thực tế.
        /// Chỉ gọi xuống Database duy nhất 1 lần để lấy Transactions, tính toán toàn bộ trên RAM.
        /// </summary>
        public async Task InitializeDashboardAsync()
        {
            try
            {
                // 1. Kiểm tra trạng thái đăng nhập
                if (UserSessionService.CurrentUser == null)
                {
                    ResetDashboardData();
                    return;
                }

                int currentUserId = UserSessionService.CurrentUser.UserID;

                // 2. Tải danh sách giao dịch của user hiện tại từ Database
                var allUserTransactions = await _transactionService.LoadDataByUserIdAsync(currentUserId);

                if (allUserTransactions == null || !allUserTransactions.Any())
                {
                    ResetDashboardData();
                    return;
                }

                // 3. Tính "TỔNG SỐ DƯ" (Cộng/trừ dồn toàn bộ giao dịch từ trước đến nay)
                GetTotalBalance = allUserTransactions.Sum(t => _transactionService.GetSignedAmount(t));

                // 4. Lọc danh sách các giao dịch thuộc THÁNG HIỆN TẠI để tính chỉ số tháng
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                var monthlyTransactions = allUserTransactions
                    .Where(t => t.TransactionDate.HasValue &&
                                t.TransactionDate.Value.Month == currentMonth &&
                                t.TransactionDate.Value.Year == currentYear)
                    .ToList();

                // 5. Tính toán "THU NHẬP THÁNG" & "CHI TIÊU THÁNG"
                decimal tempIncome = 0;
                decimal tempExpense = 0;

                foreach (var t in monthlyTransactions)
                {
                    decimal signedAmount = _transactionService.GetSignedAmount(t);
                    if (signedAmount > 0)
                    {
                        tempIncome += signedAmount;
                    }
                    else
                    {
                        tempExpense += Math.Abs(signedAmount); // Lấy trị tuyệt đối để hiển thị số dương lên thẻ UI
                    }
                }

                GetMonthlyIncome = tempIncome;
                GetMonthlyExpense = tempExpense;

                // 6. Đổ dữ liệu vào bảng "GIAO DỊCH GẦN ĐÂY TRONG THÁNG NÀY" (Sắp xếp mới nhất lên đầu, lấy tối đa 10 dòng)
                RecentTransactions.Clear();
                var recentItems = monthlyTransactions.OrderByDescending(t => t.TransactionDate).Take(10);
                foreach (var transaction in recentItems)
                {
                    RecentTransactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải dữ liệu Dashboard: {ex.Message}");
                ResetDashboardData();
            }
        }

        /// <summary>
        /// Đưa các thông số màn hình chính về 0 khi có lỗi hoặc chưa đăng nhập
        /// </summary>
        private void ResetDashboardData()
        {
            GetTotalBalance = 0;
            GetMonthlyIncome = 0;
            GetMonthlyExpense = 0;
            RecentTransactions.Clear();
        }
        #endregion
    }
}