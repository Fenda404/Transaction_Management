using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            ResultInput = _isPasswordMode ? txtPasswordInput.Password : txtInput.Text;
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
