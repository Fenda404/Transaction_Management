using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Transaction_Management.Commands;
using Transaction_Management.Models;
using Transaction_Management.Services;

namespace Transaction_Management.ViewModels
{
    public class BudgetDialogViewModel: BaseViewModel
    {
        private readonly BudgetService _budgetService = new BudgetService();
        private readonly Window _currentWindow; // Giữ tham chiếu để đóng cửa sổ sau khi lưu xong

        #region Properties
        // Đối tượng Budget đang được thao tác trên giao diện (Binding trực tiếp tới TextBox, ComboBox)
        private Budgets _currentBudget;
        public Budgets CurrentBudget
        {
            get => _currentBudget;
            set { _currentBudget = value; OnPropertyChanged(); }
        }

        // Danh sách Danh mục đổ vào ComboBox cho người dùng chọn
        private ObservableCollection<Categories> _categories = new ObservableCollection<Categories>();
        public ObservableCollection<Categories> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        // Thuộc tính lưu Danh mục được chọn trong ComboBox
        private Categories _selectedCategory;
        public Categories SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                if (_selectedCategory != null && CurrentBudget != null)
                {
                    CurrentBudget.CategoryID = _selectedCategory.CategoryID;
                }
            }
        }

        // Tiêu đề hiển thị động trên cửa sổ (Ví dụ: "THÊM NGÂN SÁCH MỚI" hoặc "CẬP NHẬT NGÂN SÁCH")
        public string DialogTitle => CurrentBudget?.BudgetID == 0 ? "THÊM NGÂN SÁCH MỚI" : "CẬP NHẬT NGÂN SÁCH";

        // Các lệnh thực thi
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Hàm khởi tạo nhận vào Window cha, UserId và đối tượng Budget được truyền từ BudgetsViewModel sang
        /// </summary>
        public BudgetDialogViewModel(Window window, int userId, Budgets budget)
        {
            _currentWindow = window;

            // Sao chép sâu hoặc gán đối tượng để chỉnh sửa dữ liệu độc lập
            CurrentBudget = budget ?? new Budgets { BudgetID = 0, UserID = userId, AmountLimit = 0 };

            // Khởi tạo các nút bấm lệnh
            SaveCommand = new RelayCommand(async _ => await ExecuteSave());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());

            // Tải danh sách Danh mục lên ComboBox
            _ = LoadCategoriesAsync();
        }
        #endregion

        #region Methods
        private async Task LoadCategoriesAsync()
        {
            var data = await _budgetService.GetCategoriesAsync();
            Categories = new ObservableCollection<Categories>(data);

            // Nếu là hành động Sửa, tự động chọn đúng phần tử danh mục cũ trên ComboBox
            if (CurrentBudget.CategoryID != 0)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.CategoryID == CurrentBudget.CategoryID);
            }
        }

        /// <summary>
        /// Logic xử lý nút LƯU (Bao gồm cả Thêm mới và Sửa đổi)
        /// </summary>
        private async Task ExecuteSave()
        {
            // 1. Kiểm tra hợp lệ dữ liệu đầu vào (Validation)
            if (CurrentBudget.CategoryID == 0 || SelectedCategory == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục chi tiêu cho ngân sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CurrentBudget.AmountLimit <= 0)
            {
                MessageBox.Show("Hạn mức ngân sách phải lớn hơn 0đ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isSuccess;

            // 2. Phân loại lệnh xử lý dựa trên ID
            if (CurrentBudget.BudgetID == 0)
            {
                // Hành động THÊM MỚI (CREATE)
                isSuccess = await _budgetService.AddAsync(CurrentBudget);
            }
            else
            {
                // Hành động CHỈNH SỬA (UPDATE)
                isSuccess = await _budgetService.UpdateAsync(CurrentBudget);
            }

            // 3. Phản hồi kết quả lên màn hình
            if (isSuccess)
            {
                if (_currentWindow != null)
                {
                    _currentWindow.DialogResult = true; // Gán bằng true để báo cho ViewModel gốc tự động Load lại dữ liệu
                    _currentWindow.Close(); // Đóng cửa sổ nhập liệu
                }
            }
            else
            {
                MessageBox.Show("Lưu dữ liệu ngân sách thất bại. Vui lòng kiểm tra lại!", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel()
        {
            if (_currentWindow != null)
            {
                _currentWindow.DialogResult = false;
                _currentWindow.Close();
            }
        }
        #endregion
}
}
