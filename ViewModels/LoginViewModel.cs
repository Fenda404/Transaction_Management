using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Transaction_Management.Commands;
using Transaction_Management.Helpers;
using Transaction_Management.Services;
using Transaction_Management.Views.MainViews;
using Transaction_Management.Views.Messages;

namespace Transaction_Management.ViewModels
{
    class LoginViewModel : BaseViewModel
    {
        #region Fields & Properties
        private readonly LoginService _loginService = new LoginService();
        private string _username;
        private bool _isLoggingIn;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        // Tối ưu hóa: Tránh lưu trữ trạng thái đăng nhập rải rác.
        // Gán trực tiếp Object User vào Session để các màn hình sau (như TransactionService) dùng chung.
        public static string CurrentUser { get; set; }
        public static string CurrentRole { get; set; }

        private int _roleID;
        public int RoleID
        {
            get => _roleID;
            set
            {
                _roleID = value;
                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(IsUser));
            }
        }

        // Tận dụng biểu thức toán tử rút gọn (Expression-bodied members)
        public bool IsAdmin => UserSessionService.IsAdmin(RoleID);
        public bool IsUser => UserSessionService.IsUser(RoleID);

        /// <summary>
        /// Trạng thái đang xử lý đăng nhập nhằm chặn người dùng nhấn nút Login liên tiếp nhiều lần
        /// </summary>
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                _isLoggingIn = value;
                OnPropertyChanged(nameof(IsLoggingIn));
                // Kích hoạt cập nhật lại trạng thái Enable/Disable của nút trên UI
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public static string CurrentThemeColor { get; set; }
        public static bool IsDarkMode { get; set; }
        public ICommand LoginCommand { get; set; }
        #endregion

        #region Constructor
        public LoginViewModel()
        {
            // Tối ưu hàm kiểm tra CanExecute: Nút đăng nhập sẽ tự động khóa lại (Disable) nếu đang trong quá trình kết nối DB
            LoginCommand = new RelayCommand<object>(
                async (p) => await LoginExecuteAsync(p),
                (p) => !IsLoggingIn
            );
        }
        #endregion

        #region Methods

        /// <summary>
        /// Xử lý sự kiện đăng nhập bất đồng bộ - Fix triệt để lỗi đơ UI khi check DB
        /// </summary>
        private async System.Threading.Tasks.Task LoginExecuteAsync(object parameter)
        {
            if (!(parameter is PasswordBox passwordBox)) return;

            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Vui lòng điền đầy đủ thông tin!");
                return;
            }

            try
            {
                // Bật trạng thái chờ (Có thể bind thuộc tính này với ProgressBar/Loading Indicator trên giao diện nếu có)
                IsLoggingIn = true;

                // TỐI ƯU GIAO TIẾP DB: Đưa tác vụ kiểm tra tài khoản chạy ngầm (Task.Run) 
                // giúp cửa sổ Login không bị đứng im khi SQL Server phản hồi chậm.
                int roleId = 0;
                bool isAuthenticated = await System.Threading.Tasks.Task.Run(() =>
                    _loginService.Authenticate(Username, password, out roleId)
                );

                if (isAuthenticated)
                {
                    // Đăng nhập thành công -> Thiết lập Session hệ thống
                    CurrentUser = Username;
                    RoleID = roleId;
                    CurrentRole = UserSessionService.GetRoleName(roleId);

                    // Giải phóng liên kết cửa sổ cha trước khi khởi tạo View mới nhằm tối ưu RAM
                    var loginWindow = Window.GetWindow(passwordBox);

                    if (CurrentRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        var adminView = new AdminView();
                        adminView.Show();
                    }
                    else
                    {
                        // CHÈN VÀO ĐÂY: Nạp cấu hình từ DB lên AppConfig trước khi giao diện chính hiện ra
                        var userService = new UserService();
                        await userService.LoadUserConfigToAppConfigAsync(Username);
                        // Gọi hàm lấy màu từ DB (Hàm ở câu hỏi trước)
                        var (userTheme, isDarkMode) = await ThemeService.GetThemeForUser(UserSessionService.CurrentUser.UserID);
                        
                        
                        CurrentThemeColor = userTheme;
                        IsDarkMode = isDarkMode;
                        ThemeHelper.ChangeTheme(CurrentThemeColor, IsDarkMode);
                        var mainView = new MainView();
                        mainView.Show();
                    }

                    // Đóng cửa sổ Login
                    loginWindow?.Close();
                }
                else
                {
                    ShowError("Đăng nhập thất bại! Vui lòng kiểm tra tên đăng nhập và mật khẩu.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hệ thống đăng nhập: {ex.Message}");
                ShowError("Đã có lỗi xảy ra trong quá trình kết nối hệ thống!");
            }
            finally
            {
                // Tắt trạng thái chờ, mở khóa lại nút bấm nếu đăng nhập thất bại
                IsLoggingIn = false;
            }
        }

        /// <summary>
        /// Hiển thị hộp thoại báo lỗi dạng Dialog chặn màn hình
        /// </summary>
        private void ShowError(string message)
        {
            ErrorDialog error = new ErrorDialog(message);
            error.ShowDialog();
        }
        #endregion
    }
}