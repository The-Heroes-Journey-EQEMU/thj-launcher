using System;
using System.Windows;
using System.Windows.Input;
using THJPatcher.Helpers;
using System.Windows.Documents;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;
using THJPatcher.Views;

namespace THJPatcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"v{version}";
            
            
            //Install Wizard
            InstallWizard.StartInstallationRequested += InstallWizard_StartInstallationRequested;
            
            // Configure Buttons
            PlayGameButton.IsEnabled = false;
            CheckForUpdateButton.IsEnabled = true;
            ProcessStatus.Document.Blocks.Clear();
            
            // Welcom Message
            var paragraph = new Paragraph();
            paragraph.TextAlignment = TextAlignment.Center;

            // Title
            paragraph.Inlines.Add(new Run("Welcome to The Heroes Journey Installer\n") 
            { 
                FontWeight = FontWeights.Bold,
                FontSize = 24
            });

            // Subtitle with less spacing
           // paragraph.Inlines.Add(new Run("\nThe Heroes Journey EverQuest Server") 
            //{ 
            //    FontStyle = FontStyles.Italic,
            //    FontSize = 20
           // });

            paragraph.Inlines.Add(new Run("\n\n\n\n\n\n\n\n\n\n"));

            // Body text
            paragraph.Inlines.Add(new Run("This tool will help you walk through the installation process.\n") { FontSize = 16 });
            paragraph.Inlines.Add(new Run("Click 'Install Game' to begin.") { FontSize = 16 });

            ProcessStatus.Document.Blocks.Add(paragraph);

            InstallWizard.Visibility = Visibility.Collapsed;
            
            // Possibly use for the future for Heavy Runs
            Task.Run(() => 
            {

            });
        }

        private async void InstallWizard_StartInstallationRequested(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Starting installation process");
                
                // Hide the wizard
                InstallWizard.Visibility = Visibility.Collapsed;
                InstallWizard.HideNavigationButtons();
                
                // Show the status
                ProcessStatus.Visibility = Visibility.Visible;
                ProcessStatus.Margin = new Thickness(60, 0, 60, 50);
                
                // Update status and start installation
                UpdateProcessStatus("Starting installation...");
                
                // Start the installation process
                await SteamHelper.InstallGame(
                    (text) => Clipboard.SetText(text),
                    (status) => UpdateProcessStatus(status)
                );

                UpdateProcessStatus("Installation completed!");
                await Task.Delay(1000);

                //Ask About Depot Cleanup
                UpdateProcessStatus("Checking for cleanup options...");
                await SteamHelper.DeleteSteamDepot();
                
                UpdateProcessStatus("All operations completed successfully!");
                UpdateProcessStatus("Welcome to The Heroes Journey!");
                UpdateProcessStatus("You may now close this window.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Installation error: {ex.Message}");
                UpdateProcessStatus($"Error during installation: {ex.Message}");
            }
        }

        private void UpdateProcessStatus(string message)
        {
            if (ProcessStatus.Document.Blocks.Count == 0)
            {
                ProcessStatus.AppendText(message);
            }
            else
            {
                ProcessStatus.AppendText($"\n{message}");
            }
            
            // Set Auto Scroll
            ProcessStatus.ScrollToEnd();
        }

        public void ChangeBackgroundImage(string imageName)
        {
            try
            {
                var backgroundImage = this.FindName("BackgroundImage") as Image;
                if (backgroundImage != null)
                {
                    var uri = new Uri($"/Images/{imageName}", UriKind.Relative);
                    backgroundImage.Source = new BitmapImage(uri);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing background: {ex.Message}");
            }
        }

        private void InstallGameButton_Click(object sender, RoutedEventArgs e)
        {
            var warningDialog = new WarningDialog();
            warningDialog.ShowDialog();

            if (warningDialog.Result)
            {
                // Hide the Install Game button
                InstallGameButton.Visibility = Visibility.Collapsed;
                
                // Clear Welcom Message
                ProcessStatus.Document.Blocks.Clear();
                
                // Show installation wizard
                InstallWizard.Visibility = Visibility.Visible;
                InstallWizard.ShowNavigationButtons();
                ProcessStatus.Visibility = Visibility.Collapsed;
            }
        }

        //installation complete or cancelled
        private void RestoreOriginalBackground()
        {
            ChangeBackgroundImage("Launcher_Window.png");
            InstallWizard.HideNavigationButtons();
            InstallWizard.Visibility = Visibility.Collapsed;
            
            InstallGameButton.Visibility = Visibility.Visible;
            ProcessStatus.Visibility = Visibility.Visible;
        }

        private void CheckForUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement update check
        }

        private void PlayGameButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement game launch
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InitializeWizard()
        {
        }
    }
}
