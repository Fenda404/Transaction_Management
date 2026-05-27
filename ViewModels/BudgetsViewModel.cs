using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Transaction_Management.Commands;
using Transaction_Management.Models;
using Transaction_Management.Services;

namespace Transaction_Management.ViewModels
{
    public class BudgetsViewModel : BaseViewModel
    {
        #region Properties

        private TransactionService _transactionService;
        private TM_Database _database;

        // Observable collection for budget list items
        private ObservableCollection<BudgetItemViewModel> _budgetList;
        public ObservableCollection<BudgetItemViewModel> BudgetList
        {
            get => _budgetList;
            set
            {
                _budgetList = value;
                OnPropertyChanged(nameof(BudgetList));
            }
        }

        // Total spent across all budgets
        private decimal _totalSpent;
        public decimal TotalSpent
        {
            get => _totalSpent;
            set
            {
                _totalSpent = value;
                OnPropertyChanged(nameof(TotalSpent));
            }
        }

        // Total budget limit across all budgets
        private decimal _totalBudgetLimit;
        public decimal TotalBudgetLimit
        {
            get => _totalBudgetLimit;
            set
            {
                _totalBudgetLimit = value;
                OnPropertyChanged(nameof(TotalBudgetLimit));
            }
        }

        // Overall progress percentage
        private double _overallProgress;
        public double OverallProgress
        {
            get => _overallProgress;
            set
            {
                _overallProgress = value;
                OnPropertyChanged(nameof(OverallProgress));
            }
        }

        #endregion

        #region Constructor

        public BudgetsViewModel()
        {
            _transactionService = new TransactionService();
            _database = new TM_Database();
            BudgetList = new ObservableCollection<BudgetItemViewModel>();
            LoadBudgets();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load all budgets for the current user from the database
        /// </summary>
        private void LoadBudgets()
        {
            try
            {
                var currentUserId = UserSessionService.CurrentUser.UserID;
                var now = DateTime.Now;

                // Get all budgets for the current user that are still valid
                var budgets = _database.Budgets
                    .Where(b => b.UserID == currentUserId && b.EndDate >= now.Date)
                    .ToList();

                BudgetList.Clear();

                foreach (var budget in budgets)
                {
                    var budgetItem = CreateBudgetItemViewModel(budget);
                    BudgetList.Add(budgetItem);
                }

                // Calculate total spent and limit
                CalculateTotals();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải ngân sách: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a BudgetItemViewModel from a Budget model
        /// </summary>
        private BudgetItemViewModel CreateBudgetItemViewModel(Budgets budget)
        {
            var categoryName = budget.Categories?.CategoryName ?? "Unknown";
            var spentAmount = CalculateSpentAmount(budget);
            var limitAmount = budget.AmountLimit;
            var usagePercentage = limitAmount > 0 ? (double)(spentAmount / limitAmount) * 100 : 0;
            var statusText = usagePercentage > 100 ? "VỀT HẠN MỨC" : "Bình thường";
            var progressColor = usagePercentage > 100 ? Brushes.Red : (usagePercentage > 80 ? Brushes.Orange : Brushes.Green);

            return new BudgetItemViewModel
            {
                BudgetID = budget.BudgetID,
                CategoryName = categoryName,
                CategoryID = budget.CategoryID,
                SpentAmount = spentAmount,
                LimitAmount = limitAmount,
                UsagePercentage = usagePercentage,
                StatusText = statusText,
                ProgressColor = progressColor,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate
            };
        }

        /// <summary>
        /// Calculate the total amount spent in a specific budget category
        /// </summary>
        private decimal CalculateSpentAmount(Budgets budget)
        {
            try
            {
                var currentUserId = UserSessionService.CurrentUser.UserID;

                // Get all transactions for this user in this category within the budget period
                var spent = _transactionService.Transactions
                    .Where(t => t.UserID == currentUserId
                            && t.CategoryID == budget.CategoryID
                            && t.TransactionDate.HasValue
                            && t.TransactionDate.Value >= budget.StartDate
                            && t.TransactionDate.Value <= budget.EndDate)
                    .Sum(t => _transactionService.GetSignedAmount(t) < 0 ? -_transactionService.GetSignedAmount(t) : 0);

                return spent;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculate totals for the summary area
        /// </summary>
        private void CalculateTotals()
        {
            TotalSpent = BudgetList.Sum(b => b.SpentAmount);
            TotalBudgetLimit = BudgetList.Sum(b => b.LimitAmount);
            OverallProgress = TotalBudgetLimit > 0 ? (double)(TotalSpent / TotalBudgetLimit) * 100 : 0;
        }

        /// <summary>
        /// Refresh the budget list (e.g., after a transaction is added)
        /// </summary>
        public void RefreshBudgets()
        {
            _transactionService.LoadData();
            LoadBudgets();
        }

        #endregion
    }

    /// <summary>
    /// View model for individual budget items shown in the UI
    /// </summary>
    public class BudgetItemViewModel : BaseViewModel
    {
        private int _budgetID;
        public int BudgetID
        {
            get => _budgetID;
            set
            {
                _budgetID = value;
                OnPropertyChanged(nameof(BudgetID));
            }
        }

        private int _categoryID;
        public int CategoryID
        {
            get => _categoryID;
            set
            {
                _categoryID = value;
                OnPropertyChanged(nameof(CategoryID));
            }
        }

        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged(nameof(CategoryName));
            }
        }

        private decimal _spentAmount;
        public decimal SpentAmount
        {
            get => _spentAmount;
            set
            {
                _spentAmount = value;
                OnPropertyChanged(nameof(SpentAmount));
            }
        }

        private decimal _limitAmount;
        public decimal LimitAmount
        {
            get => _limitAmount;
            set
            {
                _limitAmount = value;
                OnPropertyChanged(nameof(LimitAmount));
            }
        }

        private double _usagePercentage;
        public double UsagePercentage
        {
            get => _usagePercentage;
            set
            {
                _usagePercentage = value;
                OnPropertyChanged(nameof(UsagePercentage));
            }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private Brush _progressColor;
        public Brush ProgressColor
        {
            get => _progressColor;
            set
            {
                _progressColor = value;
                OnPropertyChanged(nameof(ProgressColor));
            }
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }
    }
}
