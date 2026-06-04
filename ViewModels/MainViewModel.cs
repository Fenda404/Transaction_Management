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

        private string _themeColor;
        public string ThemeColor
        {
            get => _themeColor;
            set { _themeColor = value; OnPropertyChanged(nameof(ThemeColor)); }
        }
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

        private string _getRoleName;
        public string GetRoleName
        {
            get => _getRoleName;
            set
            {
                _getRoleName = value;
                OnPropertyChanged(nameof(GetRoleName));
            }
        }

        // Lệnh chuyển đổi trang (Dashboard, Transactions, Reports, Budgets, Settings)
        public ICommand SwitchViewCommand { get; set; }
        

        public MainViewModel()
        {
            // 1. Khởi tạo trạng thái ban đầu
            // Lấy tên người dùng từ LoginViewModel sau khi đăng nhập thành công
            GetUsername = LoginViewModel.CurrentUser ?? "Admin";
            GetRoleName = LoginViewModel.CurrentRole ?? "ADMIN";
            
            // Trang mặc định khi vừa mở ứng dụng là Dashboard
            CurrentView = new DashboardViewModel();

            // 2. Khởi tạo logic chuyển trang
            // Sử dụng RelayCommand<string> để nhận tham số từ CommandParameter trong XAML
            SwitchViewCommand = new RelayCommand<string>(async (p) =>
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
                        if (CurrentView is BudgetsViewModel budgetVM)
                        {
                            // Gọi một hàm public ngoài BudgetViewModel (Chúng ta sẽ viết ở Bước 2)
                            await budgetVM.CheckBudgetViolationsOnNavigatedAsync();
                        }
                        break;
                    case "Settings":
                        CurrentView = new SettingsViewModel();
                        break;
                }
            }, (p) => true);

            
            

        }
    }
}