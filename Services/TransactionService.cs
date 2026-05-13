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
        private TM_Database db_transactions = new TM_Database();
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
    }
    #endregion
}
