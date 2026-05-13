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
        private TM_Database db_transactions = new TM_Database();
        public ObservableCollection<Transactions> Transactions { get; set; }
        public ObservableCollection<Categories> Categories { get; set; }

        public ObservableCollection<Wallets> Wallets { get; set; }
        public TransactionService()
        {
            LoadData();
        }
        public void LoadData()
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
        }
    }
}
