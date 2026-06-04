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
using System.Windows.Shapes;

namespace Transaction_Management.Views.SubViews
{
    /// <summary>
    /// Interaction logic for BudgetAlert.xaml
    /// </summary>
    public partial class BudgetAlert : Window
    {
        public BudgetAlert(string violationsText)
        {
            InitializeComponent();
            TxtViolations.Text = violationsText;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
