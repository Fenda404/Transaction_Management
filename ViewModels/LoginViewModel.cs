using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Transaction_Management.Commands;
using Transaction_Management.Services;
using Transaction_Management.Views.MainViews;
using Transaction_Management.Views.Messages;

namespace Transaction_Management.ViewModels
{
    class LoginViewModel : BaseViewModel
    {


        #region Properties
        private readonly LoginService _loginService = new LoginService();
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        public static string CurrentUser { get; set; }

        private int _roleID;
        public int RoleID
        {
            get => _roleID;
            set
            {
                _roleID = value;
                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(IsUser));
                OnPropertyChanged(nameof(CurrentRole));
            }
        }

        public bool IsAdmin => UserSessionService.IsAdmin(RoleID);
        public bool IsUser => UserSessionService.IsUser(RoleID);
        public static string CurrentRole { get; set; }


        public ICommand LoginCommand { get; set; }
        #endregion

        #region Constructor
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<object>((p) => LoginExecute(p),(p) => true);
        }
        #endregion
        #region Methods


        /// <summary>
        /// Xử lý sự kiện đăng nhập từ giao diện (View).
        /// </summary>
        /// <param name="parameter">
        /// Tham số dạng object, yêu cầu ép kiểu về <see cref="PasswordBox"/> 
        /// để lấy mật khẩu trực tiếp nhằm đảm bảo an toàn dữ liệu (không lưu Password trong bộ nhớ String quá lâu).
        /// </param>
        /// <exception cref="System.NullReferenceException">Ném ra nếu parameter không phải là một PasswordBox hợp lệ.</exception>
        private void LoginExecute(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string password = passwordBox.Password;
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter all the required information!");
                return;
            }

            if (_loginService.Authenticate(Username, password, out int roleId))
            {
                CurrentUser = Username;
                
                RoleID = roleId;
                CurrentRole = UserSessionService.GetRoleName(RoleID);

                MainView main = new MainView();
                main.Show();

                Window currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x != main);

                currentWindow?.Close();
            }
            else
            {
                ShowError("Login failed! Please check your username and password.");
            }
        }

        /// <summary>
        ///  Xử lý hiển thị nội dung khi có lỗi xảy ra
        /// </summary>
        /// <param name="message">Truyền vào nội dung lỗi</param>
        private void ShowError(string message)
        {
            ErrorDialog error = new ErrorDialog(message);
            error.ShowDialog();
        }
        #endregion
    }
}
