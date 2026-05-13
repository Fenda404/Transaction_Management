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
using Transaction_Management.Views.Messages;

namespace Transaction_Management.ViewModels
{
    public class AddTransactionViewModel:BaseViewModel
    {
        #region Properties
        TransactionService transactionService = new TransactionService();
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<Wallets> Wallets { get; set; }
        TransactionsViewModel transactionViewModel = new TransactionsViewModel();

        public bool isSaveSuccess = false;
        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        private string _wallet;
        public string Wallet
        {
            get => _wallet;
            set
            {
                _wallet = value;
                OnPropertyChanged(nameof(Wallet));
            }
        }

        private string _category;
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
        public ICommand CancelCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        #endregion
        #region Constructor
        public AddTransactionViewModel()
        {
            LoadData();
            CancelCommand = new RelayCommand<object>((p) => Cancel(p), (p) => true);
            SaveCommand = new RelayCommand(_ => Save(), _ => true);
        }
        #endregion
        #region Methods
        private void LoadData()
        {
            Wallets = transactionService.Wallets;
            // Lấy danh mục từ TransactionService thay vì hardcode
            Categories = new ObservableCollection<string>(
                transactionService.Categories.Select(c => c.CategoryName).ToList()
            );
        }

        private void Cancel(object parameter)
        {
            var window = parameter as Window;

            if (window != null)
            {
                window.Close();
            }

        }

        private void Save()
        {
            var checkSuccess = transactionService.AddTransaction(Amount, Wallet, Category, Date, Description);
            if (checkSuccess)
            {
                ConfirmDialog confirmDialog = new ConfirmDialog("Đã lưu thành công");
                confirmDialog.ShowDialog();
                isSaveSuccess = true;
            }
            else
            {
                ErrorDialog errorDialog = new ErrorDialog("Lỗi trong quá trình lưu vào database");
                errorDialog.ShowDialog();
            }

        }
        
        #endregion
    }
}
