using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Models;
namespace Transaction_Management.Services
{
    internal class TransactionService
    {
        #region Properties
        public TM_Database db_transactions = new TM_Database();
        public ObservableCollection<Transactions> Transactions { get; set; }
        public ObservableCollection<Categories> Categories { get; set; }

        public ObservableCollection<Wallets> Wallets { get; set; }

        #endregion
        #region Constructor
        public TransactionService()
        {

            LoadData();
        }
        #endregion
        #region Methods
        public List<Transactions> LoadData()
        {
            Transactions = new ObservableCollection<Transactions>(
                db_transactions.Transactions
                    .Include(t => t.Categories)
                    .Include(t => t.Wallets)
                    .Include(t => t.Users)
                    .ToList()
            );

            Categories = new ObservableCollection<Categories>(
                db_transactions.Categories.ToList()
            );

            Wallets = new ObservableCollection<Wallets>(db_transactions.Wallets.ToList());
            return Transactions.ToList();
        }

        public bool AddTransaction(decimal amount, string walletName, string categoryType, DateTime date, string description)
        {
            try
            {
                // Kiểm tra ngày giao dịch có hợp lệ không
                if (date == DateTime.MinValue)
                {
                    date = DateTime.Now;
                }

                // Lấy Wallet từ cơ sở dữ liệu dựa trên WalletName
                var wallet = db_transactions.Wallets.FirstOrDefault(w => w.WalletName == walletName);
                if (wallet == null)
                {
                    throw new Exception($"Ví '{walletName}' không tồn tại.");
                }

                // Lấy Category từ cơ sở dữ liệu dựa trên CategoryName
                var category = db_transactions.Categories.FirstOrDefault(c => c.CategoryName == categoryType);
                if (category == null)
                {
                    throw new Exception($"Danh mục '{categoryType}' không tồn tại.");
                }

                // Tạo giao dịch mới
                var transaction = new Transactions
                {
                    Amount = amount,
                    TransactionDate = date,
                    Note = description,
                    WalletID = wallet.WalletID,
                    CategoryID = category.CategoryID,
                    UserID = UserSessionService.CurrentUser.UserID
                };

                db_transactions.Transactions.Add(transaction);
                db_transactions.SaveChanges();

                // Cập nhật dữ liệu sau khi lưu thành công
                LoadData();
                return true;

            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thêm giao dịch: {ex.Message}", ex);
            }
        }

        public bool EditTransaction(int transactionID, decimal amount, string walletName, string categoryType, DateTime date, string description)
        {
            try
            {
                if (date == DateTime.MinValue)
                {
                    date = DateTime.Now;
                }
                var existingTransaction = db_transactions.Transactions.FirstOrDefault(t => t.TransactionID == transactionID);
                if (existingTransaction == null)
                {
                    throw new Exception($"Không tìm thấy giao dịch nào có ID {transactionID}");
                }

                // Lấy Wallet từ cơ sở dữ liệu dựa trên WalletName
                var wallet = db_transactions.Wallets.FirstOrDefault(w => w.WalletName == walletName);
                if (wallet == null)
                {
                    throw new Exception($"Ví '{walletName}' không tồn tại.");
                }

                // Lấy Category từ cơ sở dữ liệu dựa trên CategoryName
                var category = db_transactions.Categories.FirstOrDefault(c => c.CategoryName == categoryType);
                if (category == null)
                {
                    throw new Exception($"Danh mục '{categoryType}' không tồn tại.");
                }

                // 5. CẬP NHẬT các giá trị mới vào đối tượng đã tìm thấy (Không tạo mới)
                existingTransaction.Amount = amount;
                existingTransaction.TransactionDate = date;
                existingTransaction.Note = description;
                existingTransaction.WalletID = wallet.WalletID;
                existingTransaction.CategoryID = category.CategoryID;
                existingTransaction.UserID = UserSessionService.CurrentUser.UserID;
                db_transactions.SaveChanges();
                LoadData();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi sửa giao dịch: {ex.Message}", ex);
            }
        }

        public bool DeleteTransaction(int transactionId)
        {
            try
            {

                using (var context = new TM_Database())
                {
                    // Tìm bản ghi giao dịch trong DB dựa vào ID
                    var transaction = context.Transactions.FirstOrDefault(t => t.TransactionID == transactionId);

                    if (transaction != null)
                    {
                        context.Transactions.Remove(transaction);
                        context.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa giao dịch: {ex.Message}", ex);
                return false;
            }
        }
        public decimal GetSignedAmount(Transactions transaction)
        {
            if (transaction.Categories == null) return transaction.Amount;
            return transaction.Categories.CategoryType.ToLower() == "income" ? transaction.Amount : -transaction.Amount;
        }


        #endregion
    }
}
        
