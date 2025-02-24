using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;

namespace THJPatcher.Views
{
    public partial class SteamCheckDialog : Window
    {
        public bool Result { get; private set; }

        public SteamCheckDialog()
        {
            InitializeComponent();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
} 