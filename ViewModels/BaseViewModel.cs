using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Transaction_Management.ViewModels
{
    // CỰC KỲ QUAN TRỌNG: Phải có chữ 'public'
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}