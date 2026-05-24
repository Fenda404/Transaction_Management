using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Models;
using Transaction_Management.Services;

namespace Transaction_Management.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        #region Properties
        TransactionService transactionService = new TransactionService();

        private Transactions _transactions;
        

        private decimal _getTotalBalance;
        public decimal GetTotalBalance
        {
            get => _getTotalBalance;
            set
            {
                _getTotalBalance = value;
                OnPropertyChanged(nameof(GetTotalBalance));
            }
        }
        #endregion
        #region Constructor
        public DashboardViewModel()
        {
            TotalBalance();
        }
        #endregion
        #region Methods
        private decimal TotalBalance()
        {
            GetTotalBalance = transactionService.Transactions
                            .Where(t => t.UserID == UserSessionService.CurrentUser.UserID)
                            .Sum(t => transactionService.GetSignedAmount(t));
            return GetTotalBalance;
        }
        #endregion

    }
}
