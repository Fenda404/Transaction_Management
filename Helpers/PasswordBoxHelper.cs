using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Transaction_Management.Helpers
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty PasswordProperty =
             DependencyProperty.RegisterAttached(
                 "Password",
                 typeof(string),
                 typeof(PasswordBoxHelper),
                 new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        public static void SetPassword(DependencyObject obj, string value)
            => obj.SetValue(PasswordProperty, value);

        public static string GetPassword(DependencyObject obj)
            => (string)obj.GetValue(PasswordProperty);

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb && pb.Password != (e.NewValue as string))
                pb.Password = e.NewValue as string;
        }

        // Đính kèm sự kiện để đồng bộ từ UI lên property
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static void SetBindPassword(DependencyObject obj, bool value)
            => obj.SetValue(BindPasswordProperty, value);

        public static bool GetBindPassword(DependencyObject obj)
            => (bool)obj.GetValue(BindPasswordProperty);

        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb && (bool)e.NewValue)
            {
                pb.PasswordChanged += (sender, args) =>
                {
                    SetPassword(pb, pb.Password);
                };
            }
        }
    }
}
