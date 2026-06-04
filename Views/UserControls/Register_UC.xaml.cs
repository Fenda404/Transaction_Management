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
    /// Interaction logic for Register_UC.xaml
    /// </summary>
    public partial class Register_UC : UserControl
    {
        private readonly RegisterViewModel vm;
        public Register_UC()
        {
            InitializeComponent();
            vm = new RegisterViewModel();

            vm.GetPasswords = () => (pwbPassword.Password,pwbConfirmPass.Password);
            this.DataContext = vm;
        }
    }
}