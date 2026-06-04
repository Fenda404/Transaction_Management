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
        // Khắc phục lỗi DataContext sớm bằng cách sử dụng thuộc tính Expression nhận diện động
        public AddTransactionViewModel vm => DataContext as AddTransactionViewModel;

        public AddTransaction()
        {
            InitializeComponent();
        }
    }
}
