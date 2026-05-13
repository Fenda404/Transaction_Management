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
using Transaction_Management.ViewModels;
namespace Transaction_Management.Views.SubViews
{
    /// <summary>
    /// Interaction logic for AddTransaction.xaml
    /// </summary>
    public partial class AddTransaction : Window
    {
        AddTransactionViewModel vm;
        public AddTransaction()
        {
            InitializeComponent();
            vm = (AddTransactionViewModel)DataContext;
        }
    }
}
