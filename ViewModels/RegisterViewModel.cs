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
using Transaction_Management.Views.Messages;

namespace Transaction_Management.ViewModels
{
    
    public class RegisterViewModel : BaseViewModel
    {
        #region Properties
        private readonly RegisterService _registerService;
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

        public Func<(string, string)> GetPasswords { get; set; }

        public ICommand RegisterCommand { get; set; }
        #endregion
        #region Constructor
        public RegisterViewModel()
        {
            _registerService = new RegisterService();
            RegisterCommand = new RelayCommand(_ => Register(),_ => true);
        }
        #endregion
        #region Methods
        /// <summary>
        /// Hàm này sẽ được gọi khi người dùng nhấn nút đăng ký. Nó sẽ lấy username và password từ các trường nhập liệu, 
        /// kiểm tra tính hợp lệ của chúng, và sau đó gọi dịch vụ đăng ký để tạo tài khoản mới. Nếu có lỗi, nó sẽ hiển thị thông báo lỗi cho người dùng.
        /// </summary>
        private void Register()
        {

            var (password, confirmPassword) = GetPasswords();
            if(password != confirmPassword)
            {
                ErrorDialog err = new ErrorDialog("Password và Confirm Password không khớp.");
                err.Show();
                return;
            }

            

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ErrorDialog err = new ErrorDialog("Phải điền đầy đủ thông tin.");
                err.Show();
                return;
            }
            if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9]+$"))
            {
                ErrorDialog err = new ErrorDialog("Username chỉ được chứa chữ cái và số.");
                err.Show();
                return;
            }
            if (password.Length < 8)
            {
                ErrorDialog err = new ErrorDialog("Password phải dài hơn 8 ký tự.");
                err.Show();
                return;
            }
            bool result = _registerService.Register(Username, password);

            if (!result)
            {
                ErrorDialog err = new ErrorDialog("Tên đăng nhập đã tồn tại.");
                err.Show();
                return;
            }
            if (result)
            {
                ConfirmDialog cf = new ConfirmDialog("Đăng ký thành công! Bạn có muốn đăng nhập ngay bây giờ?");
                cf.Show();
            }
        }
        #endregion


    }
}