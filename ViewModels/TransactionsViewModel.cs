using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Transaction_Management.Commands;
using Transaction_Management.Models;
using Transaction_Management.Services;
using Transaction_Management.Views.SubViews;

namespace Transaction_Management.ViewModels
{
    public class TransactionsViewModel:BaseViewModel
    {
        #region Properties
        public ObservableCollection<Transactions> Transactions { get; set; }
        public ObservableCollection<Categories> Categories { get; set; }
        TransactionService transactionService = new TransactionService();
        public ICommand AddTransactionCommand {  get; set; }
        #endregion
        #region Constructor
        public TransactionsViewModel()
        {
            LoadData();
            
            AddTransactionCommand = new RelayCommand(_ => AddNewTransaction(), _ => true);
        }
        #endregion
        #region Methods
        private void LoadData()
        {
            
            transactionService.LoadData();
            Transactions = transactionService.Transactions;
            Categories = transactionService.Categories;
        }
        private void AddNewTransaction()
        {
            AddTransaction addTransactionView = new AddTransaction();
            bool? v = addTransactionView.ShowDialog();
        }
        #endregion
    }
}
