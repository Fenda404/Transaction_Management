using Transaction_Management.ViewModels;
using System.Windows.Controls;

namespace Transaction_Management.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Login_UC.xaml
    /// </summary>
    public partial class Login_UC : UserControl
    {
        private LoginViewModel _viewModel;

        public Login_UC()
        {
            InitializeComponent();
            _viewModel = DataContext as LoginViewModel;
        }
    }
}