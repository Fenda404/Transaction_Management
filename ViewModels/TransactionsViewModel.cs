using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Transaction_Management.Commands;
using Transaction_Management.Helpers;
using Transaction_Management.Models;
using Transaction_Management.Services;
using Transaction_Management.Views.Messages;
using Transaction_Management.Views.SubViews;

namespace Transaction_Management.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        #region Properties
        private readonly TransactionService _transactionService;

        // Danh sách chính hiển thị và binding trực tiếp lên DataGrid/ListView của UI
        private ObservableCollection<Transactions> _transactions = new ObservableCollection<Transactions>();
        public ObservableCollection<Transactions> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }

        private ObservableCollection<Categories> _categories = new ObservableCollection<Categories>();
        public ObservableCollection<Categories> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        // Biến lưu trữ tạm thời toàn bộ giao dịch gốc để phục vụ tính năng tìm kiếm (Search/Filter) lập tức trên RAM
        private List<Transactions> _allTransactionsCache = new List<Transactions>();

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _ = SearchAsync(); // Kích hoạt Debounce Search bất đồng bộ
            }
        }

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
            // Sử dụng duy nhất 1 thực thể Service xuyên suốt vòng đời màn hình
            _transactionService = new TransactionService();

            AddTransactionCommand = new RelayCommand(_ => AddNewTransaction(), _ => true);
            EditTransactionCommand = new RelayCommand(_ => EditTransaction(), _ => true);
            DeleteTransactionCommand = new RelayCommand(async _ => await DeleteTransactionAsync(), _ => true);
            AppConfig.CurrencyChanged += RefreshUI;
            // Gọi hàm khởi tạo dữ liệu bất đồng bộ an toàn
            _ = LoadDataAsync();
            _ = CheckDailyReminderOnNavigatedAsync();
        }
        #endregion

        #region Methods

        private void RefreshUI()
        {
            // 2. Báo cho XAML biết danh sách đã đổi để DataGrid chạy lại Converter
            OnPropertyChanged(nameof(Transactions));
        }

        /// <summary>
        /// Nạp dữ liệu ban đầu từ Database lên UI hoàn toàn bất đồng bộ
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                if (UserSessionService.CurrentUser == null)
                {
                    Transactions.Clear();
                    return;
                }

                int currentUserId = UserSessionService.CurrentUser.UserID;

                // Gọi trực tiếp hàm Async đã tối ưu lọc từ Database ở bước trước
                var data = await _transactionService.LoadDataByUserIdAsync(currentUserId);

                // Lưu vào bộ nhớ Cache RAM để tìm kiếm tức thì không cần gọi lại DB
                _allTransactionsCache = data ?? new List<Transactions>();

                Transactions.Clear();
                foreach (var item in _allTransactionsCache)
                {
                    Transactions.Add(item);
                }

                // Tải danh mục bất đồng bộ
                var categoriesData = await _transactionService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var category in categoriesData)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dữ liệu giao dịch: {ex.Message}");
                Transactions.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Kích hoạt popup thêm mới giao dịch
        /// </summary>
        private void AddNewTransaction()
        {
            var addTransactionView = new AddTransaction();
            var addTM = new AddTransactionViewModel();
            addTransactionView.DataContext = addTM;

            // Đăng ký callback làm mới danh sách khi cửa sổ con lưu thành công
            addTM.SavedCallback = async () => await RefreshDataAsync();
            addTransactionView.ShowDialog();
        }

        /// <summary>
        /// Làm mới lại dữ liệu nhanh chóng sau khi Thêm / Sửa / Xóa
        /// </summary>
        private async Task RefreshDataAsync()
        {
            try
            {
                IsLoading = true;
                if (UserSessionService.CurrentUser == null) return;

                // Tuyệt đối KHÔNG tạo mới 'new TransactionService()' tại đây để tránh rò rỉ kết nối DB
                var updatedData = await _transactionService.LoadDataByUserIdAsync(UserSessionService.CurrentUser.UserID);

                _allTransactionsCache = updatedData ?? new List<Transactions>();

                // Áp dụng bộ lọc tìm kiếm nếu người dùng đang gõ dở văn bản tìm kiếm
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    ApplySearchFilter();
                }
                else
                {
                    Transactions.Clear();
                    foreach (var item in _allTransactionsCache)
                    {
                        Transactions.Add(item);
                    }
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
        /// Bộ lọc tìm kiếm thông minh có Debounce hoãn tác vụ tránh xung đột khi gõ phím nhanh
        /// </summary>
        private async Task SearchAsync()
        {
            if (_allTransactionsCache == null) return;

            string currentKeyword = SearchText;

            // Chờ người dùng dừng gõ 300ms
            await Task.Delay(300);

            // Nếu người dùng vẫn đang gõ từ khóa mới thì bỏ qua luồng xử lý cũ này
            if (currentKeyword != SearchText) return;

            ApplySearchFilter();
        }

        /// <summary>
        /// Hàm nội bộ thực hiện trích xuất dữ liệu từ bộ nhớ đệm hiển thị lên giao diện
        /// </summary>
        private void ApplySearchFilter()
        {
            string keyword = SearchText;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                Transactions.Clear();
                foreach (var item in _allTransactionsCache)
                {
                    Transactions.Add(item);
                }
                return;
            }

            string keywordLower = keyword.ToLower();


            var filtered = _allTransactionsCache.Where(t =>
                (t.Note != null && t.Note.ToLower().Contains(keywordLower)) ||
                (t.Categories != null && t.Categories.CategoryName != null && t.Categories.CategoryName.ToLower().Contains(keywordLower)) ||
                t.Amount.ToString().Contains(keyword) ||
                (t.TransactionDate.HasValue && t.TransactionDate.Value.ToString("dd/MM/yyyy").Contains(keyword))
            ).ToList();

            Transactions.Clear();
            foreach (var item in filtered)
            {
                Transactions.Add(item);
            }
        }

        /// <summary>
        /// Mở màn hình cập nhật thông tin giao dịch
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
        /// Xóa giao dịch được chọn hoàn toàn bất đồng bộ
        /// </summary>
        private async Task DeleteTransactionAsync()
        {
            if (SelectedTransaction == null)
            {
                var errorDialog = new ErrorDialog("Vui lòng chọn một giao dịch để xóa!");
                errorDialog.ShowDialog();
                return;
            }

            var confirmView = new ConfirmDialog("Bạn có chắc chắn muốn xóa giao dịch này không?");
            confirmView.ShowDialog();

            // Nếu người dùng đồng ý xóa (Xác nhận từ Custom Dialog Window)
            if (confirmView.Result == true)
            {
                try
                {
                    IsLoading = true;
                    int targetId = SelectedTransaction.TransactionID;

                    // Gọi hàm Xóa bất đồng bộ trực tiếp từ tầng Service
                    var checkSuccess = await _transactionService.DeleteTransactionAsync(targetId);

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
                    var errorDialog = new ErrorDialog($"Lỗi hệ thống: {ex.Message}");
                    errorDialog.ShowDialog();
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
        public async Task CheckDailyReminderOnNavigatedAsync()
        {
            // 1. CHẶN NGAY TỪ ĐẦU: Nếu chưa có user đăng nhập thì thoát
            if (UserSessionService.CurrentUser == null) return;

            // 2. KIỂM TRA CẤU HÌNH: Người dùng có bật tính năng nhắc nhở hàng ngày không
            bool isReminderEnabled = UserSessionService.CurrentUser.IsDailyReminder;

            if (isReminderEnabled)
            {
                int userId = UserSessionService.CurrentUser.UserID;

                // 3. KIỂM TRA LOGIC: Hôm nay người dùng đã nhập giao dịch nào chưa?
                // (Gọi xuống Service để check xem hôm nay có bản ghi nào của UserId này chưa)
                bool hasEnteredToday = await _transactionService.CheckUserHasEnteredTransactionTodayAsync(userId);

                // Nếu HÔM NAY CHƯA NHẬP giao dịch nào thì mới hiện thông báo nhắc nhở
                if (!hasEnteredToday)
                {
                    // Nhường luồng 200ms cho UI vẽ xong tab để tránh gây đơ cứng ứng dụng
                    await Task.Delay(200);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        // CHẶN TRÙNG LẶP: Nếu cửa sổ nhắc nhở đang mở sẵn rồi thì không mở thêm cái nữa
                        var alreadyOpen = Application.Current.Windows.OfType<DailyReminder>().FirstOrDefault();
                        if (alreadyOpen != null)
                        {
                            alreadyOpen.Activate();
                            return;
                        }

                        // Khởi tạo cửa sổ nhắc nhở nhập liệu
                        var reminderWindow = new DailyReminder();

                        // Gán Owner để căn giữa theo App chính một cách an toàn
                        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
                        {
                            reminderWindow.Owner = Application.Current.MainWindow;
                        }

                        reminderWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        // Dùng Show() để giải phóng hoàn toàn luồng, không lo bị đơ App khi chuyển tab
                        reminderWindow.Show();

                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            #endregion
        }
    }
}