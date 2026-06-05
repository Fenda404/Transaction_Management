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
    /// Interaction logic for DailyReminder.xaml
    /// </summary>
    public partial class DailyReminder : Window
    {
        public bool IsNavigateToTransactionRequested { get; private set; } = false;
        public DailyReminder()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
