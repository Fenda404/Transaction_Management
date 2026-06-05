using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Transaction_Management.Views.Messages;

namespace Transaction_Management.Views.SubViews
{
    /// <summary>
    /// Interaction logic for ChangeInfomation.xaml
    /// </summary>
    public partial class ChangeInfomation : Window
    {
        public string ResultInput { get; private set; }
        public bool IsConfirmed { get; private set; }
        private bool _isPasswordMode;

        /// <summary>
        /// Constructor khởi tạo linh hoạt tiêu đề và chế độ nhập
        /// </summary>
        public ChangeInfomation(string title, bool isPasswordMode = false, string defaultText = "")
        {
            InitializeComponent();

            // 1. Đổi tiêu đề hiển thị trên giao diện (TextBlock)
            txtTitle.Text = title.ToUpper();

            // 2. Đổi TỰ ĐỘNG tiêu đề gốc của Window (Title) theo chuỗi truyền vào
            this.Title = title;

            _isPasswordMode = isPasswordMode;

            if (_isPasswordMode)
            {
                txtInput.Visibility = Visibility.Collapsed;
                txtPasswordInput.Visibility = Visibility.Visible;
                txtPasswordInput.Focus();
            }
            else
            {
                txtInput.Visibility = Visibility.Visible;
                txtPasswordInput.Visibility = Visibility.Collapsed;
                txtInput.Text = defaultText;
                txtInput.Focus();
                if (!string.IsNullOrEmpty(defaultText)) txtInput.SelectAll();
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (_isPasswordMode)
            {
                // Kiểm tra Password
                if (txtPasswordInput.Password.Length < 8)
                {
                    new ErrorDialog("Password phải dài hơn 8 ký tự.").Show();
                    return;
                }
                ResultInput = txtPasswordInput.Password;
            }
            else
            {
                // Kiểm tra Username
                string username = txtInput.Text.Trim();
                if (string.IsNullOrWhiteSpace(username) || !Regex.IsMatch(username, @"^[a-zA-Z0-9]+$") || username.Length < 3)
                {
                    new ErrorDialog("Username không được để trống, chỉ được chứa chữ cái và số, và phải dài ít nhất 3 ký tự.").Show();
                    return;
                }
                ResultInput = username;
            }

            IsConfirmed = true;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.DialogResult = false;
            this.Close();
        }
    }
}
