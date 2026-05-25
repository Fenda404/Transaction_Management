using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Management.ViewModels
{
    public class CategoryBreakdownItem : BaseViewModel
    {
        private string _categoryName;
        private decimal _amount;
        private double _percentage;
        private string _colorCode;

        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(nameof(CategoryName)); }
        }

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(nameof(Amount)); }
        }

        public double Percentage
        {
            get => _percentage;
            set { _percentage = value; OnPropertyChanged(nameof(Percentage)); }
        }

        public string ColorCode
        {
            get => _colorCode;
            set { _colorCode = value; OnPropertyChanged(nameof(ColorCode)); }
        }
    }
}
