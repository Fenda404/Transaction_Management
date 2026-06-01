using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Transaction_Management.Commands;
using Transaction_Management.Models;
using Transaction_Management.Views.Messages;

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
        private ObservableCollection<Budgets> _budgets;
        public ObservableCollection<Budgets> Budgets
        {
            get => _budgets;
            set
            {
                _budgets = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BudgetList)); // Cập nhật đồng thời cho giao diện cũ/mới
            }
        }

        // Tạo thuộc tính bí danh (Alias) để tương thích hoàn toàn với Budgets_UC.xaml sử dụng BudgetList
        public ObservableCollection<Budgets> BudgetList
        {
            get => Budgets;
            set => Budgets = value;
        }

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
                OnPropertyChanged(nameof(TotalBudgetLimit)); // Đồng bộ bí danh
            }
        }

        // Tạo thuộc tính bí danh cho tổng hạn mức ngân sách
        public decimal TotalBudgetLimit => TotalLimit;

        private double _overallProgress;
        public double OverallProgress
        {
            get => _overallProgress;
            set { _overallProgress = value; OnPropertyChanged(); }
        }

        // Khai báo các lệnh CRUD cho giao diện Ngân sách
        public ICommand AddBudgetCommand { get; set; }
        public ICommand EditBudgetCommand { get; set; }
        public ICommand DeleteBudgetCommand { get; set; }
        #endregion

        #region Constructor
        public BudgetsViewModel()
        {
            // Liên kết các nút bấm đến các hành động giả lập nhanh
            AddBudgetCommand = new RelayCommand(_ => ExecuteAddBudget());
            EditBudgetCommand = new RelayCommand(p => ExecuteEditBudget(p as Budgets));
            DeleteBudgetCommand = new RelayCommand(p => ExecuteDeleteBudget(p as Budgets));

            // Nạp dữ liệu giả lập
            LoadMockBudgetData();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tạo và nạp dữ liệu giả để hiển thị trực quan các dải màu tiến trình (Xanh, Cam, Đỏ)
        /// </summary>
        private void LoadMockBudgetData()
        {
            Budgets = new ObservableCollection<Budgets>();

            // 1. Ngân sách Ăn uống - Mức chi tiêu bình thường (Màu xanh lá - Dưới 80%)
            Budgets.Add(new Budgets
            {
                BudgetID = 1,
                UserID = 1,
                CategoryID = 101,
                AmountLimit = 5000000,   // Hạn mức: 5 Triệu
                SpentAmount = 2500000,   // Thực chi: 2.5 Triệu (Đạt 50%)
                Categories = new Categories { CategoryID = 101, CategoryName = "Ăn uống & Thực phẩm", CategoryType = "Expense" }
            });

            // 2. Ngân sách Thuê nhà & Điện nước - Cố định an toàn (Màu xanh lá - Dưới 80%)
            Budgets.Add(new Budgets
            {
                BudgetID = 2,
                UserID = 1,
                CategoryID = 102,
                AmountLimit = 4500000,   // Hạn mức: 4.5 Triệu
                SpentAmount = 3000000,   // Thực chi: 3 Triệu (Đạt ~66.7%)
                Categories = new Categories { CategoryID = 102, CategoryName = "Thuê nhà & Tiện ích", CategoryType = "Expense" }
            });

            // 3. Ngân sách Di chuyển, xăng xe - Mức cảnh báo (Màu cam - Từ 80% đến dưới 100%)
            Budgets.Add(new Budgets
            {
                BudgetID = 3,
                UserID = 1,
                CategoryID = 103,
                AmountLimit = 1500000,   // Hạn mức: 1.5 Triệu
                SpentAmount = 1300000,   // Thực chi: 1.3 Triệu (Đạt ~86.7%)
                Categories = new Categories { CategoryID = 103, CategoryName = "Di chuyển & Xăng xe", CategoryType = "Expense" }
            });

            // 4. Ngân sách Mua sắm & Shopping - Đã vượt định mức quá tải (Màu đỏ - Từ 100% trở lên)
            Budgets.Add(new Budgets
            {
                BudgetID = 4,
                UserID = 1,
                CategoryID = 104,
                AmountLimit = 3000000,   // Hạn mức: 3 Triệu
                SpentAmount = 3400000,   // Thực chi: 3.4 Triệu (Đạt ~113.3%)
                Categories = new Categories { CategoryID = 104, CategoryName = "Mua sắm & Quần áo", CategoryType = "Expense" }
            });

            // 5. Ngân sách Giải trí, Du lịch - Sắp chạm hạn mức (Màu cam - Từ 80% đến dưới 100%)
            Budgets.Add(new Budgets
            {
                BudgetID = 5,
                UserID = 1,
                CategoryID = 105,
                AmountLimit = 2000000,   // Hạn mức: 2 Triệu
                SpentAmount = 1800000,   // Thực chi: 1.8 Triệu (Đạt 90%)
                Categories = new Categories { CategoryID = 105, CategoryName = "Vui chơi & Giải trí", CategoryType = "Expense" }
            });

            // Tính toán các thông số tổng hợp cho Thẻ tổng quan
            CalculateTotals();
        }

        /// <summary>
        /// Cộng dồn tổng hạn mức và tổng thực chi dựa trên danh sách giả lập
        /// </summary>
        private void CalculateTotals()
        {
            if (Budgets == null || Budgets.Count == 0)
            {
                TotalSpent = 0;
                TotalLimit = 0;
                OverallProgress = 0;
                return;
            }

            TotalSpent = Budgets.Sum(b => b.SpentAmount);
            TotalLimit = Budgets.Sum(b => b.AmountLimit);
            OverallProgress = TotalLimit > 0 ? (double)(TotalSpent / TotalLimit) * 100 : 0;
        }

        #region Mock Action Handlers (Chạy thử nút bấm trên UI)
        private void ExecuteAddBudget()
        {
            // Hiển thị thông báo khi bấm nút Thêm
            var dialog = new ConfirmDialog("Hệ thống nhận diện lệnh [THÊM NGÂN SÁCH MỚI]. Bạn muốn tạo dữ liệu thử nghiệm không?");
            dialog.ShowDialog();

            if (dialog.Result == true)
            {
                // Thêm một mục ngân sách mới giả lập vào danh sách để kiểm tra tính năng cập nhật giao diện tự động
                Budgets.Add(new Budgets
                {
                    BudgetID = Budgets.Count + 1,
                    UserID = 1,
                    CategoryID = 106,
                    AmountLimit = 2500000,
                    SpentAmount = 400000,
                    Categories = new Categories { CategoryID = 106, CategoryName = "Mục thử nghiệm " + (Budgets.Count + 1), CategoryType = "Expense" }
                });

                CalculateTotals();
                new ConfirmDialog("Đã giả lập thêm ngân sách mới thành công!").ShowDialog();
            }
        }

        private void ExecuteEditBudget(Budgets target)
        {
            if (target == null) return;

            var dialog = new ConfirmDialog($"Bạn muốn giả lập sửa đổi hạn mức cho mục [{target.CategoryName}]?");
            dialog.ShowDialog();

            if (dialog.Result == true)
            {
                // Giả lập sửa đổi tăng hạn mức gấp đôi
                target.AmountLimit *= 2;

                // Refresh lại danh sách
                var temp = Budgets;
                Budgets = null;
                Budgets = temp;

                CalculateTotals();
                new ConfirmDialog("Cập nhật hạn mức giả lập thành công!").ShowDialog();
            }
        }

        private void ExecuteDeleteBudget(Budgets target)
        {
            if (target == null) return;

            var dialog = new ConfirmDialog($"Bạn có chắc chắn muốn xóa ngân sách [{target.CategoryName}]?");
            dialog.ShowDialog();

            if (dialog.Result == true)
            {
                Budgets.Remove(target);
                CalculateTotals();
                new ConfirmDialog("Đã gỡ bỏ ngân sách khỏi danh sách thành công!").ShowDialog();
            }
        }
        #endregion
        #endregion
    }
}