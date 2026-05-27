using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Transaction_Management.Commands;
using Transaction_Management.Models;
using Transaction_Management.Views.MainViews;
using Transaction_Management.Views.Messages;

namespace Transaction_Management.ViewModels
{
   
    public class SettingsViewModel : BaseViewModel
    {
        #region Properties
        // 1. Thông tin người dùng
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        // Lưu mật khẩu thực tế từ database
        private string _actualPassword;

        // Hiện/Ẩn mật khẩu
        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set { _isPasswordVisible = value; OnPropertyChanged(); }
        }

        private string _userPassword = "••••••••••••"; // Mặc định ẩn
        public string UserPassword
        {
            get => _userPassword;
            set { _userPassword = value; OnPropertyChanged(); }
        }

        // 2. Tiền tệ
        public ObservableCollection<string> Currencies { get; set; } = new ObservableCollection<string> { "VNĐ", "USD" };
        private string _selectedCurrency;
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set { _selectedCurrency = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Formats { get; set; } = new ObservableCollection<string> { "1,000.00", "1.000,00", "1 000,00" };
        private string _selectedFormat;
        public string SelectedFormat
        {
            get => _selectedFormat;
            set { _selectedFormat = value; OnPropertyChanged(); }
        }

        // 3. Giao diện
        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set { _isDarkMode = value; OnPropertyChanged(); }
        }

        // Lưu theme colors
        public ObservableCollection<string> ThemeColors { get; set; } = new ObservableCollection<string> { "Xanh lá (Mặc định)", "Xanh dương", "Cam", "Tím", "Đỏ", "Hồng" };
        private string _selectedThemeColor;
        public string SelectedThemeColor
        {
            get => _selectedThemeColor;
            set { _selectedThemeColor = value; OnPropertyChanged(); }
        }

        private Color _currentThemeColor = Color.FromRgb(46, 125, 50); // Xanh lá mặc định
        public Color CurrentThemeColor
        {
            get => _currentThemeColor;
            set { _currentThemeColor = value; OnPropertyChanged(); }
        }

        // Lưu index màu hiện tại
        private int _currentThemeColorIndex = 0;
        public int CurrentThemeColorIndex
        {
            get => _currentThemeColorIndex;
            set { _currentThemeColorIndex = value; OnPropertyChanged(); }
        }

        // 4. Thông báo
        private bool _budgetAlert;
        public bool BudgetAlert
        {
            get => _budgetAlert;
            set { _budgetAlert = value; OnPropertyChanged(); }
        }

        private bool _dailyReminder;
        public bool DailyReminder
        {
            get => _dailyReminder;
            set { _dailyReminder = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand SignOutCommand { get; set; }
        public ICommand TogglePasswordVisibilityCommand { get; set; }
        public ICommand ChangeUsernameCommand { get; set; }
        public ICommand ChangePasswordCommand { get; set; }
        public ICommand ChangeThemeColorCommand { get; set; }
        #endregion
        #region Constructor
        public SettingsViewModel()
        {
            Username = LoginViewModel.CurrentUser ?? "Admin";
            SelectedCurrency = "VNĐ";
            SelectedFormat = "1,000.00";
            SelectedThemeColor = "Xanh lá (Mặc định)";
            IsPasswordVisible = false;
            BudgetAlert = true;
            DailyReminder = true;

            // Lấy mật khẩu từ database
            LoadPasswordFromDatabase();

            // Khởi tạo Commands
            SignOutCommand = new RelayCommand(_ => SignOut(), _ => true);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            ChangeUsernameCommand = new RelayCommand(ChangeUsername);
            ChangePasswordCommand = new RelayCommand(ChangePassword);
        }
        #endregion
        #region Methods
        /// <summary>
        /// Hàm này để lấy mật khẩu thực tế của user từ database khi khởi tạo ViewModel. Nếu có lỗi hoặc không tìm thấy user, sẽ hiển thị "••••••••••••".
        /// </summary>
        private void LoadPasswordFromDatabase()
        {
            try
            {
                using (var context = new TM_Database())
                {
                    // Tìm user hiện tại trong database
                    var user = context.Users.FirstOrDefault(u => u.Username == Username);
                    
                    if (user != null)
                    {
                        _actualPassword = user.PasswordHash;
                    }
                    else
                    {
                        _actualPassword = "••••••••••••"; // Nếu không tìm thấy user
                    }
                }
            }
            catch
            {
                _actualPassword = "••••••••••••";
            }
        }

        /// <summary>
        /// Hàm này để chuyển đổi giữa việc hiển thị mật khẩu thực tế và ẩn nó đi. Khi người dùng nhấn nút toggle, 
        /// nếu mật khẩu đang bị ẩn, nó sẽ hiển thị mật khẩu thực tế; nếu đang hiển thị, nó sẽ ẩn lại bằng cách thay thế bằng "••••••••••••".
        /// </summary>
        /// <param name="parameter"></param>
        private void TogglePasswordVisibility(object parameter)
        {
            IsPasswordVisible = !IsPasswordVisible;
            if (IsPasswordVisible)
            {
                UserPassword = _actualPassword;
            }
            else
            {
                UserPassword = "••••••••••••";
            }
        }

        /// <summary>
        /// Hàm này để xử lý khi người dùng muốn đăng xuất khỏi ứng dụng. Khi người dùng nhấn nút đăng xuất, 
        /// sẽ hiển thị một hộp thoại xác nhận để đảm bảo rằng họ thực sự muốn đăng xuất. Nếu người dùng xác nhận, ứng dụng
        /// </summary>
        private void SignOut()
        {
            ConfirmDialog confirmDialog = new ConfirmDialog("Bạn chắc chắn muốn đăng xuất?");
            confirmDialog.ShowDialog();

            if (confirmDialog.Result)
            {
                MainLoginView mainLoginView = new MainLoginView();

                mainLoginView.Show();
                Application.Current.MainWindow = mainLoginView;
                Window currentWin = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x != mainLoginView);

                currentWin?.Close();
            }
        }

        /// <summary>
        /// Hàm này để xử lý khi người dùng muốn thay đổi username của họ. Khi người dùng nhấn nút thay đổi username, sẽ mở một hộp thoại hoặc giao diện mới để họ nhập username mới. 
        /// Sau đó, sẽ có logic để cập nhật username trong database và cập nhật lại thông tin hiển thị trên giao diện người dùng.
        /// </summary>
        /// <param name="parameter"></param>
        private void ChangeUsername(object parameter)
        {
            // TODO: Thêm logic để thay đổi username
        }

        /// <summary>
        /// Hàm này để xử lý khi người dùng muốn thay đổi password của họ. 
        /// Khi người dùng nhấn nút thay đổi password, sẽ mở một hộp thoại hoặc giao diện mới để họ nhập password mới.
        /// </summary>
        /// <param name="parameter"></param>
        private void ChangePassword(object parameter)
        {
            // TODO: Thêm logic để thay đổi password
        }
        #endregion
    }
}