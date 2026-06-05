using Transaction_Management.ViewModels; // Sửa namespace PhatDat thành Transaction
using System.Windows;
using System;

namespace Transaction_Management.Views.MainViews // Ép chuẩn đường dẫn
{
    public partial class MainView : Window
    {
        
        public MainView()
        {
            InitializeComponent();
            textblockDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            textblockTime.Text = DateTime.Now.ToString("HH:mm");
        }
    }
}