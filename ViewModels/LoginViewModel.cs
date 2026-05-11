using Transaction_Management.Commands;
using Transaction_Management.Views.MainViews; // Chú ý: Trỏ đúng thư mục chứa MainView
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Transaction_Management.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        // 1. Thuộc tính để Binding với ô TextBox Username
        private string _getUsername;
        public string GetUsername
        {
            get { return _getUsername; }
            set
            {
                _getUsername = value;
                OnPropertyChanged(nameof(GetUsername));
            }
        }

        public ICommand LoginCommand { get; set; }

        // Biến static để lưu tên đăng nhập dùng chung toàn ứng dụng
        public static string CurrentUser { get; set; }

        public LoginViewModel()
        {
            // Khởi tạo lệnh đăng nhập, truyền vào PasswordBox
            // Đưa điều kiện (p) => true lên trước để khớp với thư viện của bạn
            // Chỉ cần truyền hành động Login(p). Điều kiện mặc định tự động là true.
            LoginCommand = new RelayCommand<PasswordBox>((p) => Login(p));
        }

        private void Login(PasswordBox pwb)
        {
            if (pwb == null) return;

            string password = pwb.Password;

            // Kiểm tra rỗng
            if (string.IsNullOrEmpty(GetUsername) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Username và Password!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // GHI CHÚ: Sau này bạn thêm logic kiểm tra Database ở đây
            // ...
            // if (CheckDatabase(GetUsername, password) == false) return;

            // NẾU ĐĂNG NHẬP THÀNH CÔNG:

            // 1. Lưu lại tài khoản đang dùng
            CurrentUser = GetUsername;

            // 2. Mở cửa sổ trang chính (MainView)
            MainView mainView = new MainView();
            mainView.Show();

            // 3. Đóng cửa sổ Đăng nhập (MainLoginView) hiện tại
            Window loginWindow = Application.Current.Windows.OfType<MainLoginView>().FirstOrDefault();
            if (loginWindow != null)
            {
                loginWindow.Close();
            }
        }
    }
}