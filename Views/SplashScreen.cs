using System;
using System.Windows;

namespace THJPatcher.Views
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        public new void Close()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    base.Close();
                }
                catch (Exception)
                {
                }
            });
        }
    }
} 