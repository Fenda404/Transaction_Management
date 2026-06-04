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
using Transaction_Management.Views.Messages;

namespace Transaction_Management.ViewModels
{
    public class AddTransactionViewModel : BaseViewModel
    {
        #region Properties

        // Khởi tạo đối tượng Service để làm việc với DB
        private readonly TransactionService _transactionService = new TransactionService();

        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Wallets> Wallets { get; set; } = new ObservableCollection<Wallets>();

        // Loại bỏ việc khởi tạo new TransactionsViewModel() ở đây nếu không dùng tới để tránh rò rỉ bộ nhớ

        public bool isSaveSuccess = false;

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        private string _wallet;
        public string Wallet
        {
            get => _wallet;
            set
            {
                _wallet = value;
                OnPropertyChanged(nameof(Wallet));
            }
        }

        private string _category;
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public Action SavedCallback { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        #endregion

        #region Constructor
        public AddTransactionViewModel()
        {
            // Gọi hàm Load dữ liệu bất đồng bộ một cách an toàn từ Constructor
            _ = LoadDataAsync();

            CancelCommand = new RelayCommand<object>((p) => Cancel(p), (p) => true);

            // Chuyển SaveCommand thành một Action bất đồng bộ (async/await)
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => true);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Nạp danh sách Ví và Danh mục bất đồng bộ từ Service
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                // Lấy danh sách ví từ DB
                var walletsData = await _transactionService.GetWalletsAsync();
                Wallets.Clear();
                foreach (var wallet in walletsData)
                {
                    Wallets.Add(wallet);
                }

                // Lấy danh sách tên danh mục từ DB
                var categoriesData = await _transactionService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var categoryName in categoriesData.Select(c => c.CategoryName))
                {
                    Categories.Add(categoryName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp dữ liệu ComboBox: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            var window = parameter as Window;
            if (window != null)
            {
                window.Close();
            }
        }

        /// <summary>
        /// Lưu giao dịch bất đồng bộ xuống Database và phản hồi UI
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào cơ bản trước khi đẩy xuống Service
                if (string.IsNullOrEmpty(Wallet) || string.IsNullOrEmpty(Category) || Amount <= 0)
                {
                    ErrorDialog validationDialog = new ErrorDialog("Vui lòng điền đầy đủ số tiền, ví và danh mục!");
                    validationDialog.ShowDialog();
                    return;
                }

                // Gọi hàm nạp dữ liệu bất đồng bộ từ tầng Service mới viết lại
                var checkSuccess = await _transactionService.AddTransactionAsync(Amount, Wallet, Category, Date, Description);

                if (checkSuccess)
                {
                    isSaveSuccess = true;
                    ConfirmDialog confirmDialog = new ConfirmDialog("Đã lưu thành công");
                    confirmDialog.ShowDialog();

                    // Kích hoạt callback để thông báo cho View lớn bên ngoài cập nhật lại danh sách hiển thị
                    SavedCallback?.Invoke();
                }
                else
                {
                    ErrorDialog errorDialog = new ErrorDialog("Lỗi trong quá trình lưu vào database");
                    errorDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ErrorDialog errorDialog = new ErrorDialog($"Lỗi hệ thống: {ex.Message}");
                errorDialog.ShowDialog();
            }
        }
        #endregion
    }
}