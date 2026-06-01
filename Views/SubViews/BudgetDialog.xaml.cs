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
using Transaction_Management.Views.Messages;

namespace Transaction_Management.Views.SubViews
{
    public partial class BudgetDialog : Window
    {
        public Budgets BudgetResult { get; private set; }
        private bool _isEdit = false;

        public BudgetDialog(int userId, Budgets existingBudget = null)
        {
            InitializeComponent();
            LoadCategories();

            if (existingBudget != null)
            {
                _isEdit = true;
                BudgetResult = existingBudget;
                cbCategory.SelectedValue = existingBudget.CategoryID;
                cbCategory.IsEnabled = false; // Không cho đổi hạng mục khi sửa
                txtLimit.Text = existingBudget.AmountLimit.ToString("F0");
            }
            else
            {
                BudgetResult = new Budgets { UserID = userId };
            }
        }

        private void LoadCategories()
        {
            using (var db = new TM_Database())
            {
                // Chỉ thiết lập ngân sách cho các danh mục Chi tiêu (Expense)
                cbCategory.ItemsSource = db.Categories.Where(c => c.CategoryType == "Expense").ToList();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cbCategory.SelectedValue == null || string.IsNullOrWhiteSpace(txtLimit.Text))
            {
                new ErrorDialog("Vui lòng nhập đầy đủ thông tin hạn mức!").ShowDialog();
                return;
            }

            if (!decimal.TryParse(txtLimit.Text, out decimal limit) || limit <= 0)
            {
                new ErrorDialog("Số tiền hạn mức nhập vào không hợp lệ!").ShowDialog();
                return;
            }

            BudgetResult.CategoryID = (int)cbCategory.SelectedValue;
            BudgetResult.AmountLimit = limit;

            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}