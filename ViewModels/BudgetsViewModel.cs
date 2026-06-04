using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Transaction_Management.Commands;
using Transaction_Management.Helpers;
using Transaction_Management.Models;
using Transaction_Management.Services;
using Transaction_Management.Views.Messages;
using Transaction_Management.Views.SubViews;

namespace Transaction_Management.Models
{
    /// <summary>
    /// Lớp mở rộng (Partial Class) cho thực thể Budgets để cung cấp các thuộc tính hiển thị giao diện.
    /// Khắc phục hoàn toàn lỗi CS0117 và CS1061 liên quan đến SpentAmount, ProgressColor...
    /// </summary>
    public partial class Budgets
    {
        // 1. Số tiền thực tế người dùng đã chi tiêu (Có thể gán giả lập hoặc tính toán từ Database)
        private decimal _spentAmount;
        public decimal SpentAmount
        {
            get => _spentAmount;
            set => _spentAmount = value;
        }

        // 2. Số tiền còn lại trong hạn mức ngân sách
        public decimal RemainingAmount => AmountLimit - SpentAmount;

        // 3. Tỷ lệ phần trăm ngân sách đã sử dụng (%)
        public double UsagePercentage => AmountLimit > 0 ? (double)(SpentAmount / AmountLimit) * 100 : 0;

        // 4. Ánh xạ các trường dữ liệu bị lệch tên gọi giữa XAML và Database để chống lỗi Binding
        public string CategoryName => Categories?.CategoryName ?? "Không xác định";
        public string StatusText => UsagePercentage >= 100 ? "Vượt hạn mức!" : "An toàn";
        public decimal LimitAmount => AmountLimit;

        // 5. Màu sắc thanh tiến trình tự động thay đổi (Xanh lá -> Cam -> Đỏ)
        public Brush ProgressColor
        {
            get
            {
                if (UsagePercentage >= 100)
                    return new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Đỏ (Vượt định mức)
                if (UsagePercentage >= 80)
                    return new SolidColorBrush(Color.FromRgb(255, 160, 0));  // Cam (Cảnh báo gần đầy)

                return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Xanh lá (An toàn)
            }
        }
    }
}

namespace Transaction_Management.ViewModels
{
    /// <summary>
    /// ViewModel cung cấp dữ liệu giả lập (Mock Data) chất lượng cao cho Budgets_UC.xaml
    /// Giúp bạn kiểm tra thiết kế giao diện trực quan ngay lập tức mà không cần kết nối SQL Server.
    /// </summary>
    public class BudgetsViewModel : BaseViewModel
    {
        #region Properties
        private readonly BudgetService _budgetService = new BudgetService();
        private readonly TransactionService _transactionService = new TransactionService();
        public List<Categories> AllCategories { get; set; } = new List<Categories>();

        private ObservableCollection<Budgets> _budgets = new ObservableCollection<Budgets>();
        public ObservableCollection<Budgets> Budgets
        {
            get => _budgets;
            set
            {
                _budgets = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BudgetList)); // Đồng bộ cho XAML cũ sử dụng danh sách này
            }
        }

        public ObservableCollection<Budgets> BudgetList
        {
            get => Budgets;
            set => Budgets = value;
        }

        // --- Các thuộc tính hiển thị cho Khung Tổng Ngân Sách Đã Sử Dụng ở trên cùng ---
        private decimal _totalSpent;
        public decimal TotalSpent
        {
            get => _totalSpent;
            set { _totalSpent = value; OnPropertyChanged(); }
        }

        private decimal _totalLimit;
        public decimal TotalLimit
        {
            get => _totalLimit;
            set
            {
                _totalLimit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalBudgetLimit));
            }
        }

        public decimal TotalBudgetLimit => TotalLimit;

        private double _overallProgress;
        public double OverallProgress
        {
            get => _overallProgress;
            set { _overallProgress = value; OnPropertyChanged(); }
        }

        // Hệ thống Command điều hướng nút bấm
        public ICommand AddBudgetCommand { get; set; }
        public ICommand EditBudgetCommand { get; set; }
        public ICommand DeleteBudgetCommand { get; set; }
        #endregion

        #region Constructor
        public BudgetsViewModel()
        {
            // Gán lệnh hoạt động thực tế
            AddBudgetCommand = new RelayCommand(_ => ExecuteAddBudget());
            EditBudgetCommand = new RelayCommand(p => ExecuteEditBudget(p as Budgets));
            DeleteBudgetCommand = new RelayCommand(p => ExecuteDeleteBudget(p as Budgets));

            // Kích hoạt tiến trình tải dữ liệu thật bất đồng bộ từ SQL Server
            _ = LoadDataAsync();
            _ = CheckBudgetViolationsOnNavigatedAsync();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Hàm lấy dữ liệu thật từ SQL, đối chiếu thời gian thực với các giao dịch trong tháng
        /// </summary>
        public async Task LoadDataAsync()
        {
            try
            {
                // 1. Kiểm tra session đăng nhập người dùng
                if (UserSessionService.CurrentUser == null)
                {
                    Budgets.Clear();
                    ResetOverview();
                    return;
                }

                int currentUserId = UserSessionService.CurrentUser.UserID;

                // 2. Chạy song song 2 luồng DB: Lấy danh sách Ngân sách đặt ra và Toàn bộ giao dịch
                var budgetsTask = _budgetService.LoadDataByUserIdAsync(currentUserId);
                var transactionsTask = _transactionService.LoadDataByUserIdAsync(currentUserId);

                await Task.WhenAll(budgetsTask, transactionsTask);

                var dbBudgets = await budgetsTask ?? new List<Budgets>();
                var allTransactions = await transactionsTask ?? new List<Transactions>();

                // 3. Định vị mốc thời gian tháng hiện tại
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                // Lọc giao dịch chi tiêu tháng này
                var monthlyExpenses = allTransactions
                    .Where(t => t.TransactionDate.HasValue &&
                                t.TransactionDate.Value.Month == currentMonth &&
                                t.TransactionDate.Value.Year == currentYear)
                    .ToList();

                // 4. Liên kết tính toán SpentAmount trực tiếp trên RAM
                Budgets.Clear();
                foreach (var budget in dbBudgets)
                {
                    // Sum tổng số tiền của các giao dịch CHI TIÊU có CategoryID trùng khớp ngân sách này
                    decimal spent = monthlyExpenses
                        .Where(t => t.CategoryID == budget.CategoryID && _transactionService.GetSignedAmount(t) < 0)
                        .Sum(t => Math.Abs(_transactionService.GetSignedAmount(t)));

                    budget.SpentAmount = spent; // Đổ số tiền thật vào biến mở rộng
                    Budgets.Add(budget);
                }

                // 5. Cập nhật thanh tiến trình tổng quát (Hộp màu xanh nhạt phía trên UI)
                CalculateTotals();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dữ liệu thực tế: {ex.Message}");
                Budgets.Clear();
                ResetOverview();
            }
        }

        private void CalculateTotals()
        {
            if (Budgets == null || Budgets.Count == 0)
            {
                ResetOverview();
                return;
            }

            TotalSpent = Budgets.Sum(b => b.SpentAmount);
            TotalLimit = Budgets.Sum(b => b.AmountLimit);
            OverallProgress = TotalLimit > 0 ? (double)(TotalSpent / TotalLimit) * 100 : 0;
        }

        private void ResetOverview()
        {
            TotalSpent = 0;
            TotalLimit = 0;
            OverallProgress = 0;
        }

        #region CRUD Actions Logic Real
        private void ExecuteAddBudget()
        {
            try
            {
                // 1. Kiểm tra nếu chưa đăng nhập thì không mở màn hình
                if (UserSessionService.CurrentUser == null) return;

                int currentUserId = UserSessionService.CurrentUser.UserID;

                // 2. Tạo một đối tượng Budget rỗng chuẩn bị cho việc thêm mới
                Budgets newBudget = new Budgets
                {
                    UserID = currentUserId,
                    AmountLimit = 0 // Giá trị mặc định ban đầu
                };



                // 3. Khởi tạo Dialog và truyền đủ 2 tham số: UserId và đối tượng Budget mới
                BudgetDialog budgetDialog = new BudgetDialog(currentUserId, newBudget);

                // 4. Hiển thị dưới dạng Dialog (ShowDialog) để đóng băng màn hình chính, 
                // bắt buộc người dùng tương tác xong với cửa sổ Thêm mới.

                

                bool? result = budgetDialog.ShowDialog();

                // 5. Nếu người dùng nhấn "Lưu" (thường trả về true)
                if (result == true)
                {
                    // Tự động làm mới (Refresh) lại danh sách trên giao diện quản lý ngân sách từ DB thật
                    LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi mở cửa sổ thêm ngân sách: {ex.Message}");
            }
        }

        private void ExecuteEditBudget(Budgets target)
        {
            if (target == null || UserSessionService.CurrentUser == null) return;

            int currentUserId = UserSessionService.CurrentUser.UserID;

            // Truyền đối tượng ngân sách cũ cần sửa vào tham số thứ 2


            BudgetDialog budgetDialog = new BudgetDialog(currentUserId, target);
            var resultDialog = budgetDialog.ShowDialog();

            if (resultDialog == true)
            {
                
                LoadDataAsync(); // Lưu xong tự động cập nhật lại giao diện
            }
        }

        private async void ExecuteDeleteBudget(Budgets target)
        {
            if (target == null) return;

            var dialog = new ConfirmDialog($"Bạn có chắc chắn muốn xóa ngân sách danh mục [{target.CategoryName}] không?");
            dialog.ShowDialog();

            if (dialog.Result == true)
            {
                bool success = await _budgetService.DeleteAsync(target.BudgetID);
                if (success) { await LoadDataAsync(); }
            }
        }

        public async Task CheckBudgetViolationsOnNavigatedAsync()
        {
            if (UserSessionService.CurrentUser == null) return;

            bool isAlertEnabled = UserSessionService.CurrentUser.IsBudgetAlert;

            if (isAlertEnabled)
            {
                int userId = UserSessionService.CurrentUser.UserID;
                string violations = await _transactionService.CheckBudgetViolationsAsync(userId);

                if (!string.IsNullOrEmpty(violations))
                {
                    // Bắn tiến trình hiển thị vào Dispatcher chạy ở mức ưu tiên Background 
                    // để đảm bảo trang chính đã render xong hoàn toàn, tránh gây đơ UI
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        // Kiểm tra xem cửa sổ loại BudgetAlert này đã mở sẵn trên màn hình chưa
                        // để tránh việc người dùng bấm qua bấm lại nó đè thêm 4, 5 cửa sổ giống nhau
                        var alreadyOpen = Application.Current.Windows.OfType<BudgetAlert>().FirstOrDefault();

                        if (alreadyOpen != null)
                        {
                            alreadyOpen.Activate(); // Nếu mở rồi thì chỉ đẩy nó lên trước mặt, không tạo mới
                            return;
                        }

                        var alertWindow = new BudgetAlert(violations);

                        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
                        {
                            alertWindow.Owner = Application.Current.MainWindow;
                        }

                        alertWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        // THAY THẾ: Sử dụng Show() thay vì ShowDialog() để giải phóng hoàn toàn UI Thread
                        alertWindow.Show();
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }
        #endregion

        #endregion
    }
}