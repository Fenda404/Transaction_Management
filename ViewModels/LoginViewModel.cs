using Transaction_Management.Commands;
using Transaction_Management.Views;
using Transaction_Management.Views.UserControls;
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
    class LoginViewModel : BaseViewModel
    {
        public ICommand LoginCommand { get; set; }

        // Biến static để lưu tên đăng nhập dùng chung toàn ứng dụng
        public static string CurrentUser { get; set; }


    }
}
