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
        TransactionService transactionService = new TransactionService();
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<Wallets> Wallets { get; set; }
        TransactionsViewModel transactionViewModel = new TransactionsViewModel();

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
            LoadData();
            FillData();
        }
        #endregion
        #region Methods

        /// <summary>
        /// Hàm này để đổ dữ liệu từ transaction gốc vào các property của ViewModel, giúp hiển thị thông tin hiện tại của transaction trong giao diện người dùng khi mở form chỉnh sửa.
        /// </summary>
        private void FillData()
        {
            Amount = _originalTransaction.Amount;
            Date = _originalTransaction.TransactionDate.Value;
            Description = _originalTransaction.Note;

            // Ép kiểu hiển thị string tương thích với SelectedValue của ComboBox
            Wallet = _originalTransaction.Wallets?.WalletName;
            Category = _originalTransaction.Categories?.CategoryName;
            CancelCommand = new RelayCommand<object>((p) => Cancel(p), (p) => true);
            SaveCommand = new RelayCommand(_ => Save(), _ => true);
        }

        /// <summary>
        /// Hàm này để tải danh sách ví và danh mục từ TransactionService, giúp hiển thị các tùy chọn có sẵn trong ComboBox khi người dùng chỉnh sửa transaction. 
        /// Việc lấy dữ liệu từ TransactionService thay vì hardcode giúp đảm bảo tính linh hoạt và đồng bộ với dữ liệu thực tế trong ứng dụng.
        /// </summary>
        private void LoadData()
        {
            Wallets = transactionService.Wallets;
            // Lấy danh mục từ TransactionService thay vì hardcode
            Categories = new ObservableCollection<string>(
                transactionService.Categories.Select(c => c.CategoryName).ToList()
            );
        }


        /// <summary>
        /// Hàm này để xử lý sự kiện khi người dùng nhấn nút "Cancel" trong giao diện chỉnh sửa transaction. Nó nhận một tham số là đối tượng cửa sổ (Window) hiện tại,
        /// và nếu tham số này không null, nó sẽ đóng cửa sổ đó lại. Điều này cho phép người dùng thoát khỏi form chỉnh sửa mà không lưu bất kỳ thay đổi nào đã thực hiện.
        /// </summary>
        /// <param name="parameter">Đối tượng cửa sổ hiện tại</param>
        private void Cancel(object parameter)
        {
            var window = parameter as Window;

            if (window != null)
            {
                window.Close();
            }

        }

        /// <summary>
        /// Hàm này để xử lý sự kiện khi người dùng nhấn nút "Save" trong giao diện chỉnh sửa transaction. 
        /// Nó sẽ gọi phương thức EditTransaction của TransactionService để cập nhật thông tin transaction trong cơ sở dữ liệu.
        /// </summary>
        private void Save()
        {
            int targetId = _originalTransaction.TransactionID;
            var checkSuccess = transactionService.EditTransaction(targetId, Amount, Wallet, Category, Date, Description);
            if (checkSuccess)
            {
                ConfirmDialog confirmDialog = new ConfirmDialog("Đã lưu thành công");
                confirmDialog.ShowDialog();
                SavedCallback?.Invoke();

            }
            else
            {
                ErrorDialog errorDialog = new ErrorDialog("Lỗi trong quá trình lưu vào database");
                errorDialog.ShowDialog();
            }
        }
        #endregion
    }
}
