using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Transaction_Management.Commands;
using Transaction_Management.Helpers;
using Transaction_Management.Models;
using Transaction_Management.Services;
using Transaction_Management.Views.MainViews;
using Transaction_Management.Views.Messages;
using Transaction_Management.Views.SubViews;

namespace Transaction_Management.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Fields & Services
        private readonly TransactionService _transactionService = new TransactionService();
        private readonly UserService _userService = new UserService();

        private int _currentUserId;
        private bool _isLoadingData; // Cờ chặn loop: Khi nạp data từ DB lên sẽ KHÔNG ghi ngược lại DB
        private string _actualPassword = "••••••••••••";

        // 🟢 FIX LỖI: Khai báo biến ngầm lưu trữ mã màu hiện tại (Mặc định ban đầu là "0" - Xanh lá)
        private string _currentThemeCode = "0";
        #endregion

        #region Properties
        private bool _isBudgetAlert;
        public bool IsBudgetAlert
        {
            get => _isBudgetAlert;
            set
            {
                if (_isBudgetAlert == value) return;
                _isBudgetAlert = value;
                OnPropertyChanged(nameof(IsBudgetAlert));

                // Kích hoạt hàm lưu bất đồng bộ tự động mỗi khi thay đổi trạng thái tick
                _ = SaveConfigChangeAsync();
            }
        }

        private bool _isDailyReminder;
        public bool IsDailyReminder
        {
            get => _isDailyReminder;
            set
            {
                if (_isDailyReminder == value) return;
                _isDailyReminder = value;
                OnPropertyChanged(nameof(IsDailyReminder));

                // Kích hoạt hàm lưu bất đồng bộ tự động mỗi khi thay đổi trạng thái tick
                _ = SaveConfigChangeAsync();
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set { _isPasswordVisible = value; OnPropertyChanged(); }
        }

        private string _userPassword = "••••••••••••";
        public string UserPassword
        {
            get => _userPassword;
            set { _userPassword = value; OnPropertyChanged(); }
        }

        // Danh sách Tiền tệ
        public ObservableCollection<string> Currencies { get; set; } = new ObservableCollection<string> { "VNĐ", "USD" };

        private string _selectedCurrency;
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (_selectedCurrency == value) return;
                _selectedCurrency = value;
                OnPropertyChanged();

                // LƯU VÀO DATABASE: Chỉ chạy khi người dùng thao tác trên UI (không chạy lúc đang nạp trang)
                if (!_isLoadingData && _currentUserId > 0)
                {
                    AppConfig.CurrentCurrency = _selectedCurrency;
                    AppConfig.RaiseCurrencyChanged();

                    SaveCurrencyToDatabaseAsync(_currentUserId, _selectedCurrency);
                }
            }
        }

        // Danh sách Định dạng số
        public ObservableCollection<string> Formats { get; set; } = new ObservableCollection<string> { "1,000.00", "1.000,00", "1 000,00" };

        // Các thuộc tính giao diện khác
        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode == value) return;
                _isDarkMode = value;
                OnPropertyChanged(nameof(IsDarkMode));

                // Khi người dùng bấm nút gạt Sáng/Tối trên giao diện công bằng
                if (!_isLoadingData)
                {
                    // 1. Thay đổi theme giao diện ngay lập tức dựa trên mã màu đang dùng
                    ApplyTheme(_currentThemeCode, _isDarkMode);

                    // 2. Lưu trạng thái DarkMode mới xuống Database (0 hoặc 1) thông qua hàm của bạn
                    _ = SaveDarkModeToDatabaseAsync(_isDarkMode);
                }
            }
        }

        public ObservableCollection<string> ThemeColors { get; set; } = new ObservableCollection<string> { "Xanh lá (Mặc định)", "Xanh dương", "Đỏ" };

        private string _selectedThemeColor;
        public string SelectedThemeColor { get => _selectedThemeColor; set { _selectedThemeColor = value; OnPropertyChanged(); } }

        private Color _currentThemeColor = Color.FromRgb(46, 125, 50);
        public Color CurrentThemeColor { get => _currentThemeColor; set { _currentThemeColor = value; OnPropertyChanged(); } }

        private int _currentThemeColorIndex = 0;
        public int CurrentThemeColorIndex { get => _currentThemeColorIndex; set { _currentThemeColorIndex = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public ICommand SignOutCommand { get; set; }
        public ICommand TogglePasswordVisibilityCommand { get; set; }
        public ICommand ChangeUsernameCommand { get; set; }
        public ICommand ChangePasswordCommand { get; set; }
        public ICommand SaveEmailCommand { get; set; }
        public ICommand SaveFullNameCommand { get; set; }
        public ICommand DeleteAllTransactionCommand { get; set; }
        public ICommand ChangeThemeColorCommand { get; set; }
        #endregion

        #region Constructor
        public SettingsViewModel()
        {
            Username = LoginViewModel.CurrentUser ?? "Admin";

            SelectedThemeColor = "Xanh lá (Mặc định)";
            IsPasswordVisible = false;

            InitializeCommands();

            // Kích hoạt tiến trình nạp dữ liệu từ DB lên UI
            _ = InitializeDataAsync();
        }
        #endregion

        #region Methods

        private void InitializeCommands()
        {
            SignOutCommand = new RelayCommand(_ => SignOut(), _ => true);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            ChangeUsernameCommand = new RelayCommand(async _ => await ExecuteChangeUsername());
            ChangePasswordCommand = new RelayCommand(async _ => await ExecuteChangePassword());
            DeleteAllTransactionCommand = new RelayCommand(async _ => await ExecuteDeleteAllTransactions());
            SaveEmailCommand = new RelayCommand(async _ => await ExecuteSaveEmail());
            SaveFullNameCommand = new RelayCommand(async _ => await ExecuteSaveFullName());

            // Đăng ký Command đổi màu nền nhận tham số Chuỗi từ Button CommandParameter ngoài XAML
            ChangeThemeColorCommand = new RelayCommand<string>(async (param) => await ExecuteChangeThemeColor(param));
        }

        /// <summary>
        /// Xử lý logic thay đổi Theme màu từ UI và cập nhật xuống DB
        /// </summary>
        private async Task ExecuteChangeThemeColor(string param)
        {
            if (string.IsNullOrEmpty(param)) return;

            // Cập nhật lại biến toàn cục để lưu dấu vết khi bấm nút Sáng/Tối
            _currentThemeCode = param;

            // 1. Thực hiện hoán đổi giao diện ứng dụng thời gian thực
            ApplyTheme(_currentThemeCode, _isDarkMode);

            // 2. Cập nhật mã số vừa chọn ("0", "1", "2") vào Database thông qua hàm có sẵn
            await SaveThemeToDatabaseAsync(param);
        }

        /// <summary>
        /// Hàm nạp động ResourceDictionary chứa bảng màu tương ứng với màu và chế độ hiển thị
        /// </summary>
        public static void ApplyTheme(string themeCode, bool isDark)
        {
            // Đồng bộ ánh xạ: "0" -> Green, "1" -> Blue, "2" -> Red theo ý bạn
            string colorName = "Green";
            switch (themeCode)
            {
                case "0": case "Green": colorName = "Green"; break;
                case "1": case "Blue": colorName = "Blue"; break;
                case "2": case "Red": colorName = "Red"; break;
                default: colorName = "Green"; break;
            }

            string mode = isDark ? "Dark" : "Light";

            // Trường hợp file Xanh lá cũ của bạn đặt là GreenTheme.xaml và DarkGreenTheme.xaml
            string themeFile = $"{colorName}{mode}Theme.xaml";
            if (colorName == "Green")
            {
                themeFile = isDark ? "DarkGreenTheme.xaml" : "GreenTheme.xaml";
            }

            // Sửa đường dẫn Pack URI trỏ chuẩn vào thư mục Styles/Main/ thay vì thư mục cũ /Themes/
            string packUri = $"pack://application:,,,/Transaction_Management;component/Styles/Main/{themeFile}";

            try
            {
                var newThemeDict = new ResourceDictionary { Source = new Uri(packUri, UriKind.RelativeOrAbsolute) };

                // Quét ngược để gỡ bỏ file cấu hình Theme cũ đang chạy để tránh chồng chéo màu sắc
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                for (int i = dictionaries.Count - 1; i >= 0; i--)
                {
                    if (dictionaries[i].Source?.ToString().Contains("Theme.xaml") == true)
                    {
                        dictionaries.RemoveAt(i);
                    }
                }

                // Chèn bảng màu mới vào ứng dụng
                dictionaries.Add(newThemeDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi không load được Resource file Theme {themeFile}: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm tập trung nạp toàn bộ cấu hình cũ từ Database lên UI lúc mở trang / refresh
        /// </summary>
        private async Task InitializeDataAsync()
        {
            try
            {
                _isLoadingData = true; // 🟢 Bật cờ chặn: Mọi sự thay đổi thuộc tính trong lúc này KHÔNG ghi đè DB

                // 1. Đọc mật khẩu ngầm
                await Task.Run(() => LoadPasswordFromDatabase());

                // 2. Đọc thông tin cá nhân qua UserService
                var info = await _userService.LoadUserInfoAsync(Username);
                if (info != null)
                {
                    _currentUserId = info.Value.Id;
                    FullName = info.Value.Fullname;
                    Email = info.Value.Email;

                    // Đồng bộ Tiền tệ
                    string dbCurrency = !string.IsNullOrEmpty(info.Value.Currency) ? info.Value.Currency : "VNĐ";
                    _selectedCurrency = dbCurrency;
                    OnPropertyChanged(nameof(SelectedCurrency));

                    AppConfig.CurrentCurrency = dbCurrency;
                    AppConfig.RaiseCurrencyChanged();

                    // 3. Đồng bộ dữ liệu chi tiết từ SQL Server thông qua DbContext công bằng
                    using (var context = new TMDatabase())
                    {
                        var userDb = context.Users.FirstOrDefault(u => u.UserID == _currentUserId);
                        if (userDb != null)
                        {
                            // Đọc cấu hình thông báo
                            _isBudgetAlert = userDb.IsBudgetAlert;
                            _isDailyReminder = userDb.IsDailyReminder;

                            // 🟢 ĐỌC TRẠNG THÁI DARKMODE TỪ DB LÊN BIẾN
                            _isDarkMode = userDb.IsDarkMode;

                            // Đọc mã màu từ DB (Mặc định là "0")
                            _currentThemeCode = userDb.ThemeColor ?? "0";

                            // Đẩy thông báo thay đổi lên UI để cập nhật lại trạng thái dấu Tick và nút gạt
                            OnPropertyChanged(nameof(IsBudgetAlert));
                            OnPropertyChanged(nameof(IsDailyReminder));
                            OnPropertyChanged(nameof(IsDarkMode));

                            // Áp dụng giao diện đồng bộ lúc khởi tạo trang
                            ApplyTheme(_currentThemeCode, _isDarkMode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi khởi tạo dữ liệu: {ex.Message}");
            }
            finally
            {
                _isLoadingData = false; // 🟢 Tắt cờ chặn: Bây giờ người dùng click thật mới ghi nhận DB
            }
        }

        private void LoadPasswordFromDatabase()
        {
            try
            {
                using (var context = new TMDatabase())
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == Username);
                    _actualPassword = user != null ? user.PasswordHash : "••••••••••••";
                }
            }
            catch
            {
                _actualPassword = "••••••••••••";
            }
        }

        private async void SaveCurrencyToDatabaseAsync(int userId, string currency)
        {
            try
            {
                bool result = await _userService.UpdateCurrencyByUserIdAsync(userId, currency);
                if (!result) System.Diagnostics.Debug.WriteLine("Lưu tiền tệ thất bại.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu Currency: {ex.Message}");
            }
        }

        private void TogglePasswordVisibility(object parameter)
        {
            IsPasswordVisible = !IsPasswordVisible;
            UserPassword = IsPasswordVisible ? _actualPassword : "••••••••••••";
        }

        private void SignOut()
        {
            ConfirmDialog confirmDialog = new ConfirmDialog("Bạn chắc chắn muốn đăng xuất?");
            confirmDialog.ShowDialog();

            if (confirmDialog.Result)
            {
                MainLoginView mainLoginView = new MainLoginView();
                mainLoginView.Show();

                var oldWindows = Application.Current.Windows.OfType<Window>().Where(w => w != mainLoginView).ToList();
                Application.Current.MainWindow = mainLoginView;

                foreach (var window in oldWindows) window.Close();
            }
        }

        private async Task ExecuteChangeUsername()
        {
            var inputDialog = new ChangeInfomation("Thay đổi tên tài khoản", false, Username);
            inputDialog.ShowDialog();

            if (inputDialog.IsConfirmed && !string.IsNullOrWhiteSpace(inputDialog.ResultInput))
            {
                string newUsername = inputDialog.ResultInput.Trim();
                if (newUsername.Equals(Username, StringComparison.OrdinalIgnoreCase)) return;

                bool success = await _userService.ChangeUsernameAsync(Username, newUsername);
                if (success)
                {
                    LoginViewModel.CurrentUser = newUsername;
                    Username = newUsername;
                    new ConfirmDialog("Thay đổi tên tài khoản thành công!").ShowDialog();
                }
                else
                {
                    new ConfirmDialog("Tên tài khoản đã tồn tại hoặc có lỗi xảy ra!").ShowDialog();
                }
            }
        }

        private async Task ExecuteChangePassword()
        {
            var inputDialog = new ChangeInfomation("Thay đổi mật khẩu", true);
            inputDialog.ShowDialog();

            if (inputDialog.IsConfirmed && !string.IsNullOrWhiteSpace(inputDialog.ResultInput))
            {
                string newPassword = inputDialog.ResultInput.Trim();
                bool success = await _userService.ChangePasswordAsync(Username, newPassword);

                if (success)
                {
                    _actualPassword = newPassword;
                    if (IsPasswordVisible) UserPassword = _actualPassword;
                    new ConfirmDialog("Thay đổi mật khẩu thành công!").ShowDialog();
                }
                else
                {
                    new ConfirmDialog("Thay đổi mật khẩu thất bại!").ShowDialog();
                }
            }
        }

        private async Task ExecuteDeleteAllTransactions()
        {
            var confirm = new ConfirmDialog("Bạn có chắc chắn muốn xóa tất cả giao dịch không?");
            confirm.ShowDialog();

            if (confirm.Result == true)
            {
                var sure = new ConfirmDialog("Hành động này sẽ xóa toàn bộ giao dịch của bạn, hãy lưu ý!");
                sure.ShowDialog();

                if (sure.Result == true)
                {
                    bool isSuccess = await _transactionService.DeleteAllTransactionsAsync();
                    new ConfirmDialog(isSuccess ? "Đã xóa tất cả giao dịch thành công!" : "Xóa tất cả giao dịch thất bại!").ShowDialog();
                }
            }
        }

        private async Task ExecuteSaveEmail()
        {
            string patternEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (Email != null && !Regex.IsMatch(Email, patternEmail))
            {
                new ConfirmDialog("Định dạng Email không hợp lệ!").ShowDialog();
                return;
            }
            if (string.IsNullOrWhiteSpace(Email)) return;

            var confirm = new ConfirmDialog("Bạn có chắn muốn thay đổi Email?");
            confirm.ShowDialog();
            if (confirm.Result == true)
            {
                bool success = await _userService.UpdateEmailAsync(Username, Email.Trim());
                new ConfirmDialog(success ? "Đã lưu Email thành công!" : "Lưu Email thất bại!").ShowDialog();
            }
        }

        private async Task ExecuteSaveFullName()
        {
            if (string.IsNullOrWhiteSpace(FullName)) return;

            var confirm = new ConfirmDialog("Bạn có chắn muốn thay đổi họ và tên?");
            confirm.ShowDialog();
            if (confirm.Result == true)
            {
                bool success = await _userService.UpdateFullNameAsync(Username, FullName.Trim());
                new ConfirmDialog(success ? "Đã lưu họ và tên thành công!" : "Lưu Họ và tên thất bại!").ShowDialog();
            }
        }

        private async Task SaveConfigChangeAsync()
        {
            try
            {
                if (!_isLoadingData && _currentUserId > 0)
                {
                    bool success = await _userService.UpdateUserSettingsAsync(_currentUserId, IsBudgetAlert, IsDailyReminder);

                    if (success)
                    {
                        if (UserSessionService.CurrentUser != null)
                        {
                            UserSessionService.CurrentUser.IsBudgetAlert = IsBudgetAlert;
                            UserSessionService.CurrentUser.IsDailyReminder = IsDailyReminder;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hệ thống khi lưu cấu hình thông báo: {ex.Message}");
            }
        }

        private async Task SaveThemeToDatabaseAsync(string themeName)
        {
            if (_isLoadingData || _currentUserId <= 0) return;

            bool success = await _userService.UpdateThemeColorAsync(_currentUserId, themeName);

            if (success && UserSessionService.CurrentUser != null)
            {
                UserSessionService.CurrentUser.ThemeColor = themeName;
            }
        }

        private async Task SaveDarkModeToDatabaseAsync(bool isDark)
        {
            if (_isLoadingData || _currentUserId <= 0) return;

            string darkModeCode = isDark ? "1" : "0";
            bool success = await _userService.UpdateDarkModeAsync(_currentUserId, darkModeCode);

            if (success && UserSessionService.CurrentUser != null)
            {
                UserSessionService.CurrentUser.IsDarkMode = isDark;
            }
        }
        #endregion
    }
}