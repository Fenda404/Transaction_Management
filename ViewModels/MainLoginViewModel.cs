using Transaction_Management.Commands;
using Transaction_Management.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Transaction_Management.ViewModels
{
    public class MainLoginViewModel : BaseViewModel
    {
        #region Properties
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }
        public ICommand SwitchViewCommand { get; set; }
        #endregion
        #region Constructor
        public MainLoginViewModel()
        {
            // Lúc đầu mới mở app thì hiện Login
            CurrentView = new LoginViewModel();

            SwitchViewCommand = new RelayCommand<string>((p) => {
                if (p == "Register")
                    CurrentView = new RegisterViewModel();
                else if (p == "Login")
                    CurrentView = new LoginViewModel();
            });
        }
        #endregion
    }
}