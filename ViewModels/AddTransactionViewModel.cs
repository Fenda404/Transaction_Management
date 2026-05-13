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

namespace Transaction_Management.ViewModels
{
    internal class AddTransactionViewModel:BaseViewModel
    {
        #region Properties
        TransactionService transactionService = new TransactionService();
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<Wallets> Wallets { get; set; }
        public ICommand CancelCommand { get; set; }
        #endregion
        #region Constructor
        public AddTransactionViewModel()
        {

            LoadData();
        }
        #endregion
        #region Methods
        private void LoadData()
        {
            Wallets = transactionService.Wallets;
            Categories = new ObservableCollection<string> { "Thu nhập", "Chi tiêu" };
            CancelCommand = new RelayCommand<object>((p) => Cancel(p), (p) => true);

        }

        private void Cancel(object parameter)
        {
            var window = parameter as Window;

            // Kiểm tra xem window có thực sự tồn tại không trước khi đóng
            if (window != null)
            {
                window.Close();
            }

        }
        #endregion
    }
}
