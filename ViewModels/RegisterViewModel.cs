using Transaction_Management.Commands;
using Transaction_Management.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using Transaction_Management.Services;
using Transaction_Management.Views.UserControls;
using Transaction_Management.Validators;

namespace Transaction_Management.ViewModels
{
    
    public class RegisterViewModel : BaseViewModel
    {
        #region Properties
        private readonly RegisterService _registerService = new RegisterService();
        private readonly ValidPassword _validPassword = new ValidPassword();
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }
        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(nameof(ConfirmPassword)); }
        }

        public ICommand RegisterCommand { get; set; }
        #endregion
        public RegisterViewModel()
        {
           RegisterCommand = new RelayCommand<object>((p) => RegisterExecute(p), (p) => true);
        }
        #region Constructor
        #endregion
        #region Methods

        private void RegisterExecute(object parameter)
        {
            // Lấy mật khẩu trực tiếp từ ViewModel (đã được cập nhật qua binding PasswordBoxHelper)
            string passwordText = Password;
            string confirmPassText = ConfirmPassword;

            // 1. Kiểm tra thông tin đầy đủ
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(passwordText) ||
                string.IsNullOrWhiteSpace(confirmPassText))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. Kiểm tra mật khẩu khớp
            if (passwordText != confirmPassText)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3. Kiểm tra độ mạnh mật khẩu
            if (!_validPassword.IsPasswordStrong(passwordText))
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 4. Thực hiện đăng ký (service sẽ hash mật khẩu trước khi lưu)
            bool isRegistered = _registerService.Register(Username, passwordText);
            if (isRegistered)
            {
                MessageBox.Show("Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Xóa trắng form
                Username = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
            }
            else
            {
                MessageBox.Show("Đăng ký thất bại. Vui lòng thử lại với tên người dùng khác.",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}