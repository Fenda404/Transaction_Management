using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Services;
namespace Transaction_Management.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly ReportService _reportService;
        private bool _isLoading;
        private string _errorMessage;
        private decimal _totalIncome;
        private decimal _totalExpense;
        private decimal _netSavings;
        private ObservableCollection<CategoryBreakdownItem> _categoryBreakdown;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public decimal TotalIncome
        {
            get => _totalIncome;
            set { _totalIncome = value; OnPropertyChanged(nameof(TotalIncome)); }
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set { _totalExpense = value; OnPropertyChanged(nameof(TotalExpense)); }
        }

        public decimal NetSavings
        {
            get => _netSavings;
            set { _netSavings = value; OnPropertyChanged(nameof(NetSavings)); }
        }

        public ObservableCollection<CategoryBreakdownItem> CategoryBreakdown
        {
            get => _categoryBreakdown;
            set { _categoryBreakdown = value; OnPropertyChanged(nameof(CategoryBreakdown)); }
        }

        public ReportsViewModel()
        {
            _reportService = new ReportService();
            LoadReportDataAsync();
        }

        private async void LoadReportDataAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                int currentUserId = UserSessionService.CurrentUser?.UserID ?? 0;
                if (currentUserId == 0)
                {
                    ErrorMessage = "Vui lòng đăng nhập để xem báo cáo.";
                    ResetData();
                    return;
                }

                // Chạy bất đồng bộ để không block UI
                var (totalIncome, totalExpense, breakdown) = await Task.Run(() =>
                    _reportService.GetReportData(currentUserId));

                TotalIncome = totalIncome;
                TotalExpense = totalExpense;
                NetSavings = totalIncome - totalExpense;

                CategoryBreakdown = new ObservableCollection<CategoryBreakdownItem>(breakdown);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không thể tải dữ liệu báo cáo: {ex.Message}";
                ResetData();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetData()
        {
            TotalIncome = 0;
            TotalExpense = 0;
            NetSavings = 0;
            CategoryBreakdown = new ObservableCollection<CategoryBreakdownItem>();
        }
    }
}
