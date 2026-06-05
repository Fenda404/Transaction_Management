using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Transaction_Management.Commands;
using Transaction_Management.Models;
using Transaction_Management.Services;
using Transaction_Management.Views.MainViews;
using Transaction_Management.Views.Messages;

namespace Transaction_Management.ViewModels
{
    internal class AdminViewModel : BaseViewModel
    {
        #region Properties
        private readonly AdminService _adminService = new AdminService();

        // Danh sách hiển thị trên DataGrid
        private ObservableCollection<Users> _userList = new ObservableCollection<Users>();
        public ObservableCollection<Users> UserList
        {
            get => _userList;
            set { _userList = value; OnPropertyChanged(); }
        }

        // Dòng được chọn trên DataGrid
        private Users _selectedUser;
        public Users SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();

                // Gọi hàm nạp dữ liệu lên Form
                LoadSelectedUserToForm();
            }
        }

        // Các thuộc tính Binding trực tiếp với Form nhập liệu bên phải
        private string _formUsername;
        public string FormUsername
        {
            get => _formUsername;
            set { _formUsername = value; OnPropertyChanged(); }
        }

        private string _formEmail;
        public string FormEmail
        {
            get => _formEmail;
            set { _formEmail = value; OnPropertyChanged(); }
        }

        private string _formPassword;
        public string FormPassword
        {
            get => _formPassword;
            set { _formPassword = value; OnPropertyChanged(); }
        }

        private int _formRoleIndex = 1; // Mặc định chọn Thành viên (User)
        public int FormRoleIndex
        {
            get => _formRoleIndex;
            set { _formRoleIndex = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand CreateUserCommand { get; set; }
        public ICommand UpdateRoleCommand { get; set; }
        public ICommand DeleteUserCommand { get; set; }

        public ICommand SignOutCommand { get; set; }
        #endregion

        #region Constructor
        public AdminViewModel()
        {
            // Khởi tạo các Command
            CreateUserCommand = new RelayCommand(async (p) => await ExecuteCreateUser());
            UpdateRoleCommand = new RelayCommand(async (p) => await ExecuteUpdateRole());
            DeleteUserCommand = new RelayCommand(async (p) => await ExecuteDeleteUser());
            SignOutCommand = new RelayCommand((p) => SignOut());

            // Tự động nạp dữ liệu khi mở màn hình
            _ = LoadUsersAsync();
        }
        #endregion

        #region Methods (CRUD Execution)

        /// <summary>
        /// Hàm nạp danh sách tài khoản từ Service lên RAM
        /// </summary>
        public async Task LoadUsersAsync()
        {
            try
            {
                var rawUsers = await _adminService.LoadUsersIdAsync();
                if (rawUsers != null)
                {
                    UserList = new ObservableCollection<Users>(rawUsers);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải danh sách Admin: {ex.Message}");
            }
        }

        /// <summary>
        /// Hành động: TẠO TÀI KHOẢN MỚI
        /// </summary>
        private async Task ExecuteCreateUser()
        {
            // 1. Validate dữ liệu đầu vào cơ bản
            if (string.IsNullOrWhiteSpace(FormUsername) || string.IsNullOrWhiteSpace(FormEmail))
            {
                MessageBox.Show("Vui lòng điền đầy đủ Tên đăng nhập và Email!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Tạo đối tượng User mới với dữ liệu mặc định
            var newUser = new Users
            {
                Username = FormUsername.Trim(),
                Email = FormEmail.Trim(),
                RoleID = FormRoleIndex == 0 ? 1 : 2,
                Currency = "VNĐ",
                IsBudgetAlert = true,
                IsDailyReminder = true,
                ThemeColor = "0",
                IsDarkMode = false
            };

            // 3. Xử lý mật khẩu (Gợi ý: Nên dùng thư viện Hash mật khẩu thay vì text thô ở đây)
            string rawPassword = !string.IsNullOrWhiteSpace(FormPassword) ? FormPassword.Trim() : "123";
            newUser.PasswordHash = rawPassword; // Ví dụ dùng BCrypt, nếu lưu thô thì dùng: newUser.PasswordHash = rawPassword;

            try
            {
                // 4. Gọi Service để thêm vào DB
                bool isSuccess = await _adminService.AddUserAsync(newUser);

                if (isSuccess)
                {
                    MessageBox.Show("Tạo tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    await LoadUsersAsync(); // Load lại danh sách hiển thị trên UI
                }
                else
                {
                    MessageBox.Show("Tạo tài khoản thất bại! (Tên đăng nhập hoặc Email có thể đã tồn tại)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi hệ thống: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Hành động: CẬP NHẬT QUYỀN & THÔNG TIN
        /// </summary>
        private async Task ExecuteUpdateRole()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản từ danh sách bảng để cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedUser.Email = FormEmail.Trim();
            SelectedUser.RoleID = FormRoleIndex == 0 ? 1 : 2;

            if (!string.IsNullOrWhiteSpace(FormPassword) && FormPassword != "********")
            {
                SelectedUser.PasswordHash = FormPassword;
            }

            bool isSuccess = await _adminService.UpdateUserAsync(SelectedUser);

            if (isSuccess)
            {
                MessageBox.Show("Cập nhật thông tin tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadUsersAsync();
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Hành động: XÓA TÀI KHOẢN
        /// </summary>
        private async Task ExecuteDeleteUser()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn tài khoản muốn xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa tài khoản '{SelectedUser.Username}' không?",
                                          "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                var userToDelete = SelectedUser;
                var (isSuccess, message) = await _adminService.DeleteUserAsync(userToDelete.UserID);

                if (isSuccess)
                {
                    MessageBox.Show("Xóa tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    UserList.Remove(userToDelete);
                }
                else
                {
                    // Hiển thị lỗi chi tiết từ service
                    MessageBox.Show($"Xóa tài khoản thất bại!\nLý do: {message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Tự động điền thông tin tài khoản được chọn vào Form bên phải
        /// </summary>
        private void LoadSelectedUserToForm()
        {
            // SỬA TẠI ĐÂY: Sử dụng biến private _selectedUser để kiểm tra thay vì thuộc tính Public công khai
            if (_selectedUser == null)
            {
                // Chỉ xóa trắng dữ liệu text trên form nhập liệu, tuyệt đối KHÔNG gọi hàm ClearForm() ở đây nữa
                FormUsername = string.Empty;
                FormEmail = string.Empty;
                FormPassword = string.Empty;
                FormRoleIndex = 1;
                return;
            }

            FormUsername = _selectedUser.Username;
            FormEmail = _selectedUser.Email;
            FormPassword = "********";
            FormRoleIndex = _selectedUser.RoleID == 1 ? 0 : 1;
        }

        /// <summary>
        /// Làm sạch các ô nhập liệu sau khi thao tác xong
        /// </summary>
        private void ClearForm()
        {
            FormUsername = string.Empty;
            FormEmail = string.Empty;
            FormPassword = string.Empty;
            FormRoleIndex = 1;

            
            _selectedUser = null;
            OnPropertyChanged(nameof(SelectedUser));
        }

        private void SignOut()
        {
            ConfirmDialog confirmDialog = new ConfirmDialog("Bạn chắc chắn muốn đăng xuất?");
            confirmDialog.ShowDialog();

            
            if (confirmDialog.Result)
            {
                
                MainLoginView mainLoginView = new MainLoginView();
                mainLoginView.Show();

                
                var oldWindows = Application.Current.Windows.OfType<Window>()
                    .Where(w => w != mainLoginView)
                    .ToList();

                
                Application.Current.MainWindow = mainLoginView;

                
                foreach (var window in oldWindows)
                {
                    window.Close();
                }
            }
        }
        #endregion
    }
}