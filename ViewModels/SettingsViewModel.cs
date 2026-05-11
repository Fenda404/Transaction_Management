using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Transaction_Management.Commands;

namespace Transaction_Management.ViewModels
{
    // CỰC KỲ QUAN TRỌNG: Chỉ dùng 1 dòng namespace và class phải là public
    public class SettingsViewModel : BaseViewModel
    {
        // 1. Thông tin người dùng
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        // 2. Tiền tệ
        public ObservableCollection<string> Currencies { get; set; } = new ObservableCollection<string> { "VNĐ", "USD" };
        private string _selectedCurrency;
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set { _selectedCurrency = value; OnPropertyChanged(); }
        }

        // 3. Giao diện
        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set { _isDarkMode = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ThemeColors { get; set; } = new ObservableCollection<string> { "Xanh lá (Mặc định)", "Xanh dương", "Cam", "Tím" };
        private string _selectedThemeColor;
        public string SelectedThemeColor
        {
            get => _selectedThemeColor;
            set { _selectedThemeColor = value; OnPropertyChanged(); }
        }

        public SettingsViewModel()
        {
            Username = LoginViewModel.CurrentUser ?? "Admin";
            SelectedCurrency = "VNĐ";
            SelectedThemeColor = "Xanh lá (Mặc định)";
        }
    }
}