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
    public class EditTransactionViewModel : BaseViewModel
    {
        #region Properties

        private readonly Transactions _originalTransaction;
        private readonly TransactionService _transactionService = new TransactionService();

        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Wallets> Wallets { get; set; } = new ObservableCollection<Wallets>();

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
        public EditTransactionViewModel(Transactions transactionToEdit)
        {
            _originalTransaction = transactionToEdit ?? throw new ArgumentNullException(nameof(transactionToEdit));

            // 1. Đăng ký các Lệnh điều hướng
            CancelCommand = new RelayCommand<object>((p) => Cancel(p), (p) => true);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => true);

            // 2. Điền dữ liệu cũ có sẵn lên giao diện lập tức
            FillData();

            // 3. Chạy ngầm tiến trình nạp danh sách ComboBox từ cơ sở dữ liệu
            _ = LoadDataAsync();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Hàm này để đổ dữ liệu từ transaction gốc vào các property của ViewModel, giúp hiển thị thông tin hiện tại của transaction trong giao diện người dùng khi mở form chỉnh sửa.
        /// </summary>
        private void FillData()
        {
            Amount = _originalTransaction.Amount;
            Date = _originalTransaction.TransactionDate ?? DateTime.Now;
            Description = _originalTransaction.Note;

            // Ép kiểu hiển thị string tương thích với SelectedValue của ComboBox
            Wallet = _originalTransaction.Wallets?.WalletName;
            Category = _originalTransaction.Categories?.CategoryName;
        }

        /// <summary>
        /// Hàm này để tải danh sách ví và danh mục từ TransactionService dưới dạng bất đồng bộ, giúp hiển thị các tùy chọn có sẵn trong ComboBox khi người dùng chỉnh sửa transaction. 
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                // Nạp danh sách ví
                var walletsData = await _transactionService.GetWalletsAsync();
                Wallets.Clear();
                foreach (var wallet in walletsData)
                {
                    Wallets.Add(wallet);
                }

                // Nạp danh sách danh mục
                var categoriesData = await _transactionService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var categoryName in categoriesData.Select(c => c.CategoryName))
                {
                    Categories.Add(categoryName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải danh mục chỉnh sửa: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm này để xử lý sự kiện khi người dùng nhấn nút "Cancel" trong giao diện chỉnh sửa transaction.
        /// </summary>
        private void Cancel(object parameter)
        {
            var window = parameter as Window;
            if (window != null)
            {
                window.Close();
            }
        }

        /// <summary>
        /// Hàm này để xử lý sự kiện khi người dùng nhấn nút "Save" dưới dạng bất đồng bộ (async/await).
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                // Kiểm tra dữ liệu hợp lệ cơ bản trước khi xử lý cập nhật
                if (string.IsNullOrEmpty(Wallet) || string.IsNullOrEmpty(Category) || Amount <= 0)
                {
                    ErrorDialog validationDialog = new ErrorDialog("Vui lòng nhập đầy đủ thông tin số tiền, ví và danh mục thích hợp!");
                    validationDialog.ShowDialog();
                    return;
                }

                int targetId = _originalTransaction.TransactionID;

                // Gọi hàm Edit bất đồng bộ từ Service
                var checkSuccess = await _transactionService.EditTransactionAsync(targetId, Amount, Wallet, Category, Date, Description);

                if (checkSuccess)
                {
                    isSaveSuccess = true;
                    ConfirmDialog confirmDialog = new ConfirmDialog("Đã cập nhật giao dịch thành công");
                    confirmDialog.ShowDialog();

                    // Kích hoạt callback thông báo cập nhật UI cho màn hình danh sách chính
                    SavedCallback?.Invoke();
                }
                else
                {
                    ErrorDialog errorDialog = new ErrorDialog("Lỗi trong quá trình cập nhật vào database");
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