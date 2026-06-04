using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Transaction_Management.Models;
using Transaction_Management.ViewModels;
using Transaction_Management.Views.Messages;

namespace Transaction_Management.Views.SubViews
{
    public partial class BudgetDialog : Window
    {
        BudgetDialogViewModel vm;
        public BudgetDialog(int userId, Budgets existingBudget = null)
        {
            InitializeComponent();
            vm = new BudgetDialogViewModel(this, userId, existingBudget);
            this.DataContext = vm;
        }

        
    }
}