using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Transaction_Management.Commands;
using Transaction_Management.Models;
using Transaction_Management.Services;
using Transaction_Management.Views.Messages;
using Transaction_Management.Views.SubViews;

namespace Transaction_Management.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        #region Properties
        private ObservableCollection<Transactions> _transactions;
        public ObservableCollection<Transactions> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }

        private ObservableCollection<Categories> _categories;
        public ObservableCollection<Categories> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        private TransactionService _transactionService;

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                Search();
            }
        }

        private ObservableCollection<Transactions> _getAllTransactions;
        private int _getTransactionID;
        public int GetTransactionID
        {
            get => _getTransactionID;
            set
            {
                _getTransactionID = value;
                OnPropertyChanged(nameof(GetTransactionID));
            }
        }

        private Transactions _selectedTransaction;
        public Transactions SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                _selectedTransaction = value;
                OnPropertyChanged(nameof(SelectedTransaction));

                if (_selectedTransaction != null)
                {
                    GetTransactionID = _selectedTransaction.TransactionID;
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand AddTransactionCommand { get; set; }
        public ICommand EditTransactionCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        #endregion
        #region Constructor
        public TransactionsViewModel()
        {

            _transactionService = new TransactionService();
            Transactions = new ObservableCollection<Transactions>();
            Categories = new ObservableCollection<Categories>();
            _getAllTransactions = new ObservableCollection<Transactions>();

            AddTransactionCommand = new RelayCommand(_ => AddNewTransaction(), _ => true);
            EditTransactionCommand = new RelayCommand(_ => EditTransaction(), _ => true);
            DeleteTransactionCommand = new RelayCommand(_ => DeleteTransaction(), _ => true);

            LoadData();
        }
        #endregion
        #region Methods


        /// <summary>
        /// Hàm này dùng để load dữ liệu theo UserID của người dùng hiện tại
        /// Sắp xếp theo TransactionDate giảm dần để giao dịch mới nhất hiển thị lên đầu
        /// Sử dụng dịch vụ của UserSessionService để lấy thông tin người dùng hiện tại và TransactionService để lấy dữ liệu giao dịch từ database
        /// </summary>
        private async void LoadData()
        {
            try
            {
                IsLoading = true;

                // Kiểm tra đăng nhập
                if (UserSessionService.CurrentUser == null)
                {
                    Transactions.Clear();
                    return;
                }

                // Chạy trên thread riêng để không đơ giao diện
                var data = await Task.Run(() =>
                {
                    return _transactionService.LoadData()
                        .Where(t => t.UserID == UserSessionService.CurrentUser.UserID)
                        .OrderByDescending(t => t.TransactionDate)
                        .ToList();
                });

                // Cập nhật UI trên main thread
                _getAllTransactions = new ObservableCollection<Transactions>(data);

                Transactions.Clear();
                foreach (var item in data)
                {
                    Transactions.Add(item);
                }

                // Load categories
                var categories = await Task.Run(() => _transactionService.Categories);
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dữ liệu: {ex.Message}");
                Transactions.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Hàm này dùng để thêm mới một giao dịch
        /// Mở một cửa sổ mới (AddTransaction) để người dùng nhập thông tin giao dịch mới
        /// </summary>

        private void AddNewTransaction()
        {
            var addTransactionView = new AddTransaction();
            var addTM = new AddTransactionViewModel();
            addTransactionView.DataContext = addTM;
            addTM.SavedCallback = async () => await RefreshDataAsync();
            addTransactionView.ShowDialog();
        }


        /// <summary>
        /// Hàm này để refresh lại dữ liệu sau khi thêm, sửa hoặc xóa giao dịch
        /// </summary>
        /// <returns></returns>
        private async Task RefreshDataAsync()
        {
            try
            {
                IsLoading = true;

                // Load lại dữ liệu mới
                var updatedData = await Task.Run(() =>
                {
                    _transactionService = new TransactionService();
                    return _transactionService.LoadData()
                        .Where(t => t.UserID == UserSessionService.CurrentUser.UserID)
                        .OrderByDescending(t => t.TransactionDate)
                        .ToList();
                });

                // Cập nhật collection
                _getAllTransactions = new ObservableCollection<Transactions>(updatedData);

                Transactions.Clear();
                foreach (var item in updatedData)
                {
                    Transactions.Add(item);
                }

                // Nếu đang có từ khóa tìm kiếm thì áp dụng lại
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    ApplySearchFilter();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi refresh dữ liệu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// Hàm này dùng để tìm kiếm giao dịch dựa trên biến SearchText
        /// </summary>
        private async void Search()
        {
            if (_getAllTransactions == null || IsLoading) return;

            string currentKeyword = SearchText;
            bool hasKeyword = !string.IsNullOrWhiteSpace(currentKeyword);

            // Tránh search quá nhiều khi gõ nhanh
            await Task.Delay(300); // Debounce 300ms

            // Kiểm tra lại keyword vì có thể đã thay đổi trong lúc delay
            if (currentKeyword != SearchText) return;

            try
            {
                IsLoading = true;

                var filteredList = await Task.Run(() =>
                {
                    if (!hasKeyword)
                    {
                        return _getAllTransactions.ToList();
                    }
                    else
                    {
                        string keywordLower = currentKeyword.ToLower();
                        return _getAllTransactions
                            .Where(t =>
                                (t.Note != null && t.Note.ToLower().Contains(keywordLower)) ||
                                (t.Categories != null && t.Categories.CategoryType != null &&
                                 t.Categories.CategoryType.ToLower().Contains(keywordLower)) ||
                                t.Amount.ToString().Contains(currentKeyword) ||
                                t.TransactionDate.ToString().Contains(currentKeyword)
                            )
                            .ToList();
                    }
                });

                // Cập nhật UI
                Transactions.Clear();
                foreach (var item in filteredList)
                {
                    Transactions.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tìm kiếm: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Hàm này dùng để áp dụng bộ lọc tìm kiếm dựa trên SearchText lên collection Transactions
        /// </summary>
        private void ApplySearchFilter()
        {
            if (_getAllTransactions == null) return;

            string currentKeyword = SearchText;
            bool hasKeyword = !string.IsNullOrWhiteSpace(currentKeyword);

            if (!hasKeyword)
            {
                Transactions.Clear();
                foreach (var item in _getAllTransactions)
                {
                    Transactions.Add(item);
                }
            }
            else
            {
                string keywordLower = currentKeyword.ToLower();
                var filtered = _getAllTransactions.Where(t =>
                    (t.Note != null && t.Note.ToLower().Contains(keywordLower)) ||
                    (t.Categories != null && t.Categories.CategoryType != null &&
                     t.Categories.CategoryType.ToLower().Contains(keywordLower)) ||
                    t.Amount.ToString().Contains(currentKeyword) ||
                    t.TransactionDate.ToString().Contains(currentKeyword)
                ).ToList();

                Transactions.Clear();
                foreach (var item in filtered)
                {
                    Transactions.Add(item);
                }
            }
        }


        /// <summary>
        /// Hàm này dùng để sửa một giao dịch đã chọn
        /// </summary>
        private void EditTransaction()
        {
            if (SelectedTransaction == null)
            {
                var errorDialog = new ErrorDialog("Vui lòng chọn một giao dịch để sửa!");
                errorDialog.ShowDialog();
                return;
            }

            var editTransactionView = new EditTransaction();
            var editTM = new EditTransactionViewModel(SelectedTransaction);
            editTransactionView.DataContext = editTM;
            editTM.SavedCallback = async () => await RefreshDataAsync();
            editTransactionView.ShowDialog();
        }


        /// <summary>
        /// Hàm này dùng để xóa một giao dịch đã chọn
        /// </summary>
        private async void DeleteTransaction()
        {
            if (SelectedTransaction == null)
            {
                var errorDialog = new ErrorDialog("Vui lòng chọn một giao dịch để xóa!");
                errorDialog.ShowDialog();
                return;
            }

            var confirmView = new ConfirmDialog("Bạn có chắc chắn muốn xóa giao dịch này không?");
            confirmView.ShowDialog();

            if (confirmView.Result == true)
            {
                try
                {
                    IsLoading = true;

                    int targetId = SelectedTransaction.TransactionID;
                    var checkSuccess = await Task.Run(() => _transactionService.DeleteTransaction(targetId));

                    if (checkSuccess)
                    {
                        var successDialog = new ConfirmDialog("Đã xóa giao dịch thành công!");
                        successDialog.ShowDialog();
                        await RefreshDataAsync();
                    }
                    else
                    {
                        var errorDialog = new ErrorDialog("Lỗi trong quá trình xóa dữ liệu khỏi database!");
                        errorDialog.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    var errorDialog = new ErrorDialog($"Lỗi: {ex.Message}");
                    errorDialog.ShowDialog();
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }


        #endregion
    }
}
