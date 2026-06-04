using Transaction_Management.ViewModels;
using System.Windows.Controls;
using System.Windows;
using System.Text;

namespace Transaction_Management.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Login_UC.xaml
    /// </summary>
    public partial class Login_UC : UserControl
    {
        private LoginViewModel _viewModel;

        public Login_UC()
        {
            InitializeComponent();
            _viewModel = DataContext as LoginViewModel;
        }

        private void btnForgetPass_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var alert = new StringBuilder();
            alert.Append("Tạm thời chưa hỗ trợ chức năng quên mật khẩu! Vui lòng đăng nhập tài khoản với quyền quản trị viên\n");
            alert.Append("Username: Administrator\n");
            alert.Append("Password: Admin@!123\n");
            alert.Append("Hoặc liên hệ Email: datdesign256@gmail.com\n");
            alert.Append("---------------------------- TRÂN TRỌNG ----------------------------");
            
            MessageBox.Show( $"{alert.ToString()}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}