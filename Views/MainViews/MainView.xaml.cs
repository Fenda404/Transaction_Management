using Transaction_Management.ViewModels; // Sửa namespace PhatDat thành Transaction
using System.Windows;

namespace Transaction_Management.Views.MainViews // Ép chuẩn đường dẫn
{
    public partial class MainView : Window
    {
        MainViewModel mvm;
        public MainView()
        {
            InitializeComponent();
            mvm = (MainViewModel)DataContext;
        }
    }
}