using System.Windows;

namespace THJPatcher.Views
{
    public partial class WarningDialog : Window
    {
        public bool Result { get; private set; }

        public WarningDialog()
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
    }
} 