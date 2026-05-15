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
using Transaction_Management.Views.Messages;
using Transaction_Management.Views.SubViews;

namespace Transaction_Management.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        #region Properties
        public ObservableCollection<Transactions> Transactions { get; set; }
        public ObservableCollection<Categories> Categories { get; set; }
        TransactionService transactionService = new TransactionService();

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                Search();
            }
        }



        private ObservableCollection<Transactions> GetAllTransactions;
        public int GetTransactionID {  get; set; }
        private Transactions _selectedTransaction;
        public Transactions SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                _selectedTransaction = value;
                OnPropertyChanged(nameof(SelectedTransaction));

                // Cập nhật lại ID (nếu bạn vẫn cần dùng biến GetTransactionID ở nơi khác)
                if (_selectedTransaction != null)
                {
                    GetTransactionID = _selectedTransaction.TransactionID; // Giả định trường ID tên là TransactionID hoặc Id
                }
            }
        }

        public ICommand AddTransactionCommand { get; set; }
        public ICommand EditTransactionCommand { get; set; }

        public ICommand DeleteTransactionCommand { get; set; }
        #endregion
        #region Constructor
        public TransactionsViewModel()
        {

            LoadData();
            AddTransactionCommand = new RelayCommand(_ => AddNewTransaction(), _ => true);
            EditTransactionCommand = new RelayCommand(_ => EditTransaction(), _ => true);
            DeleteTransactionCommand = new RelayCommand(_ => DeleteTransaction(), _ => true);
        }
        #endregion
        #region Methods
        private void LoadData()
        {

            var data = transactionService.LoadData();
            if (data != null)
            {

                GetAllTransactions = new ObservableCollection<Transactions>(data);

                Transactions = new ObservableCollection<Transactions>(data);
            }
            Categories = transactionService.Categories;
        }
        private void AddNewTransaction()
        {
            AddTransaction addTransactionView = new AddTransaction();
            var addTM = new AddTransactionViewModel();
            addTransactionView.DataContext = addTM;
            addTM.SavedCallback = () => RefreshData();
            bool? v = addTransactionView.ShowDialog();
        }

        private void RefreshData()
        {
            transactionService = new TransactionService();
            var updatedData = transactionService.LoadData();
            if (updatedData != null)
            {
                GetAllTransactions = new ObservableCollection<Transactions>(updatedData);

                Transactions.Clear();
                foreach (var item in updatedData)
                {
                    Transactions.Add(item);
                }
            }
        }

        private void Search()
        {
            if (GetAllTransactions == null) return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {

                Transactions.Clear();
                foreach (var item in GetAllTransactions)
                {
                    Transactions.Add(item);
                }
            }
            else
            {
                string keyword = SearchText.ToLower();
                var filtered = GetAllTransactions.Where(t =>
            // Tìm trong Ghi chú (Note)
            (t.Note != null && t.Note.ToLower().Contains(keyword)) ||


            (t.Categories?.CategoryType != null && t.Categories.CategoryType.ToLower().Contains(keyword)) ||

            
            (t.Amount.ToString().Contains(SearchText)) ||

            (t.TransactionDate.ToString().Contains(SearchText))


        );

                Transactions.Clear();
                foreach (var item in filtered)
                {
                    Transactions.Add(item);
                }
            }
            OnPropertyChanged(nameof(Transactions));
        }

        private void EditTransaction()
        {
            // Kiểm tra xem người dùng đã chọn dòng nào trên danh sách chưa
            if (SelectedTransaction == null)
            {
                ErrorDialog errorDialog = new ErrorDialog("Vui lòng chọn một giao dịch để sửa!");
                errorDialog.ShowDialog();
                return;
            }
            EditTransaction editTransactionView = new EditTransaction();
            var editTM = new EditTransactionViewModel(SelectedTransaction);
            editTransactionView.DataContext = editTM;
            editTM.SavedCallback = () =>RefreshData();
            bool? v = editTransactionView.ShowDialog();
        }

        private void DeleteTransaction()
        {
            if (SelectedTransaction == null)
            {
                ErrorDialog errorDialog = new ErrorDialog("Vui lòng chọn một giao dịch để xóa!");
                errorDialog.ShowDialog();
                return;
            }
            ConfirmDialog confirmView = new ConfirmDialog("Bạn có chắc chắn muốn xóa giao dịch này không?");
            confirmView.ShowDialog();

            if (confirmView.Result == true)
            {
                int targetId = SelectedTransaction.TransactionID;

                var checkSuccess = transactionService.DeleteTransaction(targetId);

                if (checkSuccess)
                {
                    ConfirmDialog successDialog = new ConfirmDialog("Đã xóa giao dịch thành công!");
                    successDialog.ShowDialog();


                    RefreshData();
                }
                else
                {
                    ErrorDialog errorDialog = new ErrorDialog("Lỗi trong quá trình xóa dữ liệu khỏi database!");
                    errorDialog.ShowDialog();
                }
            }
        }
        #endregion
    }
}
