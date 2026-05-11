using System.Windows;
using System.Windows.Input;
using System.Linq;
using Transaction_Management.Commands;
using Transaction_Management.Views.MainViews;

namespace Transaction_Management.ViewModels
{
    /// <summary>
    /// ViewModel chính điều khiển toàn bộ logic điều hướng và trạng thái của MainView.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        // View hiện tại đang được hiển thị trong ContentControl của MainView
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        // Tên người dùng hiển thị trên Sidebar
        private string _username;
        public string GetUsername
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(GetUsername));
            }
        }

        // Lệnh chuyển đổi trang (Dashboard, Transactions, Reports, Budgets, Settings)
        public ICommand SwitchViewCommand { get; set; }

        // Lệnh đăng xuất
        public ICommand SignOutCommand { get; set; }

        public MainViewModel()
        {
            // 1. Khởi tạo trạng thái ban đầu
            // Lấy tên người dùng từ LoginViewModel sau khi đăng nhập thành công
            GetUsername = LoginViewModel.CurrentUser ?? "Fenda Admin";

            // Trang mặc định khi vừa mở ứng dụng là Dashboard
            CurrentView = new DashboardViewModel();

            // 2. Khởi tạo logic chuyển trang
            // Sử dụng RelayCommand<string> để nhận tham số từ CommandParameter trong XAML
            SwitchViewCommand = new RelayCommand<string>((p) =>
            {
                switch (p)
                {
                    case "Dashboard":
                        CurrentView = new DashboardViewModel();
                        break;
                    case "Transactions":
                        CurrentView = new TransactionsViewModel();
                        break;
                    case "Reports":
                        CurrentView = new ReportsViewModel();
                        break;
                    case "Budgets":
                        CurrentView = new BudgetsViewModel();
                        break;
                    case "Settings":
                        CurrentView = new SettingsViewModel();
                        break;
                }
            }, (p) => true);

            // 3. Khởi tạo logic Đăng xuất
            SignOutCommand = new RelayCommand<object>((p) =>
            {
                // Mở lại cửa sổ Đăng nhập
                MainLoginView loginWindow = new MainLoginView();
                loginWindow.Show();

                // Tìm và đóng cửa sổ MainView hiện tại
                Window currentWin = Application.Current.Windows.OfType<MainView>().FirstOrDefault();
                if (currentWin != null)
                {
                    currentWin.Close();
                }
            });
        }
    }
}