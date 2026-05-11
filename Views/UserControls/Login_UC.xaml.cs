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
using Transaction_Management.ViewModels;

namespace Transaction_Management.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Login_UC.xaml
    /// </summary>
    public partial class Login_UC : UserControl
    {
        LoginViewModel vm;
        public Login_UC()
        {
            InitializeComponent();
            vm = (LoginViewModel)DataContext;
        }
    }
}
