using System.Windows;

namespace THJPatcher.Views
{
    public partial class ConfirmationDialog : Window
    {
        public bool Result { get; private set; }

        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
} 