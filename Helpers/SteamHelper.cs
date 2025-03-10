using System;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Net.Http;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using THJPatcher.Views;

namespace THJPatcher.Helpers
{
    public static class SteamHelper
    {
        private const string GAME_APP_ID = "205710";
        private const string GAME_DEPOT_ID = "205711";
        private const string GAME_MANIFEST_ID = "1926608638440811669";
        private const double TOTAL_SIZE_MB = 8910.0;
        private const double EXPECTED_DOWNLOAD_SIZE_MB = 8910.0;

        // Shortcut creation
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);

        // Static HttpClient
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task InstallGame(Action<string> setClipboardText, Action<string> updateStatusCallback)
        {
            try
            {
                // Get all available drives
                DriveInfo[] drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed).ToArray();
                
                // Create drive selection dialog
                var dialog = new Window
                {
                    Title = "Select Installation Drive",
                    Width = 300,
                    Height = 250,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.ToolWindow,
                    Background = new SolidColorBrush(Color.FromRgb(32, 32, 32))
                };

                var stackPanel = new StackPanel { Margin = new Thickness(10) };

                var titleText = new TextBlock 
                { 
                    Text = "Select installation drive:", 
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(212, 175, 55)),
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var spaceText = new TextBlock
                {
                    Text = "Installation requires approximately 20GB of available storage space",
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(212, 175, 55)),
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var comboBox = new ComboBox
                {
                    Margin = new Thickness(0, 10, 0, 10),
                    Width = 200,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                    Foreground = new SolidColorBrush(Color.FromRgb(212, 175, 55))
                };

                // Add drives
                foreach (var drive in drives)
                {
                    string freeSpace = (drive.TotalFreeSpace / (1024.0 * 1024 * 1024)).ToString("F1");
                    comboBox.Items.Add($"{drive.Name} ({freeSpace} GB free)");
                }
                comboBox.SelectedIndex = 0;

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var buttonStyle = new Style(typeof(Button));
                buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 45, 45))));
                buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(Color.FromRgb(212, 175, 55))));
                buttonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(212, 175, 55))));
                buttonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(20, 5, 20, 5)));

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 75,
                    Margin = new Thickness(0, 0, 10, 0),
                    IsDefault = true,
                    Style = buttonStyle
                };

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 75,
                    IsCancel = true,
                    Style = buttonStyle
                };

                string selectedDrive = null;
                okButton.Click += (s, e) =>
                {
                    if (comboBox.SelectedItem != null)
                    {
                        selectedDrive = drives[comboBox.SelectedIndex].Name;
                        dialog.DialogResult = true;
                    }
                };

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);
                
                stackPanel.Children.Add(titleText);
                stackPanel.Children.Add(spaceText);
                stackPanel.Children.Add(comboBox);
                stackPanel.Children.Add(buttonPanel);
                dialog.Content = stackPanel;

                if (dialog.ShowDialog() == true && selectedDrive != null)
                {
                    string installPath = Path.Combine(selectedDrive, "THJ");
                    updateStatusCallback($"Installing to: {installPath}");
                    updateStatusCallback("\n");
                    
                    Directory.CreateDirectory(installPath);

                    // First message
                    updateStatusCallback("Steam will launch automatically to begin the download.");
                    updateStatusCallback("\n");

                    // Command line instructions
                    updateStatusCallback("Once Steam opens:");
                    updateStatusCallback("1. Click on the command line");
                    updateStatusCallback("2. Press Ctrl + V to paste");
                    updateStatusCallback("3. Press Enter to start the download");
                    updateStatusCallback("\n");

                    // Final step message
                    updateStatusCallback("After the download completes, the installer will apply the final patches.");
                    updateStatusCallback("The installer will alert you when the process is complete.");

                    // Check multiple possible Steam locations
                    string[] possibleSteamPaths = new string[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                        "C:\\Steam",
                        "D:\\Steam"
                    };

                    string steamPath = null;
                    foreach (var path in possibleSteamPaths)
                    {
                        if (Directory.Exists(path))
                        {
                            steamPath = path;
                            break;
                        }
                    }

                    // If Steam isn't found, prompt user to locate it
                    if (steamPath == null)
                    {
                        updateStatusCallback("Steam installation not found in default locations.");
                        updateStatusCallback("Please select your Steam installation folder...");
                        
                        var dialogSteam = new Microsoft.Win32.OpenFolderDialog
                        {
                            Title = "Select Steam Installation Folder",
                            InitialDirectory = "C:\\"
                        };

                        if (dialogSteam.ShowDialog() == true)
                        {
                            steamPath = dialogSteam.FolderName;
                            if (!File.Exists(Path.Combine(steamPath, "steam.exe")))
                            {
                                throw new DirectoryNotFoundException("Selected folder does not contain Steam installation.");
                            }
                        }
                        else
                        {
                            throw new OperationCanceledException("Steam folder selection cancelled.");
                        }
                    }

                    string expectedPath = Path.Combine(steamPath, "steamapps", "content", "app_205710", "depot_205711");
                    updateStatusCallback($"Using Steam path: {steamPath}");

                    // Prepare the command
                    string command = $"download_depot {GAME_APP_ID} {GAME_DEPOT_ID} {GAME_MANIFEST_ID}";
                    
                    // Copy command to clipboard and provide clear instructions
                    setClipboardText(command);
                    updateStatusCallback("=== INSTALLATION STEPS ===");
                    updateStatusCallback("1. Steam console will open automatically");
                    updateStatusCallback("2. The download command has been copied to your clipboard");
                    updateStatusCallback("3. Click in the Steam console and press Ctrl+V to paste the command");
                    updateStatusCallback("4. Press Enter to start the download");
                    updateStatusCallback("5. Please wait while the download completes...");
                    updateStatusCallback("6. The installer will monitor and prompt you for install once its done.");
                    updateStatusCallback("========================");

                    // Open Steam console
                    updateStatusCallback("Opening Steam console...");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "steam://open/console",
                        UseShellExecute = true
                    });

                    // Monitor the download directory
                    updateStatusCallback("Waiting for download to start...");
                    bool downloadStarted = false;
                    double lastReportedSize = 0;
                    int noChangeCount = 0;
                    DateTime lastChangeTime = DateTime.Now;
                    const double TOTAL_GB = 8.91;

                    while (true)
                    {
                        await Task.Delay(5000); 

                        if (Directory.Exists(expectedPath))
                        {
                            if (!downloadStarted)
                            {
                                downloadStarted = true;
                                updateStatusCallback("Download has started!");
                            }

                            // Calculate download size
                            double currentSize = await Task.Run(() => CalculateDirectorySize(expectedPath));
                            double downloadedGB = Math.Round(currentSize / (1024.0 * 1024.0 * 1024.0), 2);
                            double progress = (downloadedGB / TOTAL_GB) * 100;
                            
                            updateStatusCallback($"Downloaded: {downloadedGB:F2} GB of {TOTAL_GB:F2} GB ({progress:F1}%)");

                            if (currentSize > lastReportedSize)
                            {
                                lastChangeTime = DateTime.Now;
                                noChangeCount = 0;
                                lastReportedSize = currentSize;
                            }
                            else
                            {
                                noChangeCount++;
                                var timeWithoutChange = DateTime.Now - lastChangeTime;
                                
                                // Check for pause dynamically
                                if (downloadedGB < (TOTAL_GB * 0.95))
                                {
                                    updateStatusCallback("Download paused, reconnecting...");
                                }
                                else
                                {
                                    updateStatusCallback($"Checking download completion... ({60 - timeWithoutChange.Seconds} seconds remaining)");
                                }

                                // Check for file download completion
                                if (noChangeCount >= 12)
                                {
                                    updateStatusCallback("Download complete!");
                                    updateStatusCallback("Transferring files...");
                                    
                                    await Task.Run(() => CopyDirectory(expectedPath, installPath));
                                    updateStatusCallback("File transfer complete!");

                                    // Download and install the patcher
                                    updateStatusCallback("Installing the Heroes Journey Patcher...");
                                    string patcherPath = Path.Combine(installPath, "heroesjourneyeq.exe");

                                    try
                                    {
                                        using (var response = await httpClient.GetAsync(
                                            "https://github.com/The-Heroes-Journey-EQEMU/eqemupatcher/releases/latest/download/heroesjourneyeq.exe",
                                            HttpCompletionOption.ResponseHeadersRead))
                                        {
                                            response.EnsureSuccessStatusCode();
                                            using (var fileStream = File.Create(patcherPath))
                                            using (var downloadStream = await response.Content.ReadAsStreamAsync())
                                            {
                                                await downloadStream.CopyToAsync(fileStream);
                                            }
                                        }
                                        updateStatusCallback("Patcher installed successfully!");

                                        
                                        if (File.Exists(patcherPath))
                                        {
                                            try
                                            {
                                                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                                                string shortcutPath = Path.Combine(desktopPath, "Heroes Journey EQ.lnk");
                                                
                                                // Create a temporary VBS script to create the shortcut
                                                string vbsScript = Path.Combine(Path.GetTempPath(), "CreateShortcut.vbs");
                                                string vbsContent = $@"
                                                    Set oWS = WScript.CreateObject(""WScript.Shell"")
                                                    sLinkFile = ""{shortcutPath}""
                                                    Set oLink = oWS.CreateShortcut(sLinkFile)
                                                    oLink.TargetPath = ""{patcherPath}""
                                                    oLink.WorkingDirectory = ""{Path.GetDirectoryName(patcherPath)}""
                                                    oLink.Save";
                                                
                                                File.WriteAllText(vbsScript, vbsContent);
                                                
                                                // Execute the VBS script
                                                Process.Start(new ProcessStartInfo
                                                {
                                                    FileName = "wscript.exe",
                                                    Arguments = $"\"{vbsScript}\"",
                                                    UseShellExecute = false,
                                                    CreateNoWindow = true
                                                }).WaitForExit();
                                                
                                                // Clean up the temporary script
                                                if (File.Exists(vbsScript))
                                                {
                                                    File.Delete(vbsScript);
                                                }
                                                
                                                updateStatusCallback("Desktop shortcut created!");
                                            }
                                            catch (Exception ex)
                                            {
                                                updateStatusCallback($"Could not create shortcut: {ex.Message}");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        updateStatusCallback($"Could not install patcher: {ex.Message}");
                                    }

                                    // Cleanup Steam download
                                    updateStatusCallback("Cleaning up temporary files...");
                                    try
                                    {
                                        await Task.Run(() => Directory.Delete(expectedPath, true));
                                    }
                                    catch (Exception ex)
                                    {
                                        updateStatusCallback($"Note: Could not delete Steam download folder: {ex.Message}");
                                        updateStatusCallback("You may want to manually delete it to free up space.");
                                    }

                                    updateStatusCallback("Installation complete!");
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    updateStatusCallback("Installation cancelled. Please try again.");
                    return;
                }
            }
            catch (Exception ex)
            {
                updateStatusCallback($"Error: {ex.Message}");
                throw;
            }
        }

        private static double CalculateDirectorySize(string path)
        {
            double size = 0;
            try
            {
                var dir = new DirectoryInfo(path);
                foreach (FileInfo fi in dir.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += fi.Length;
                }
            }
            catch (Exception)
            {
                
            }
            return size;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDir, destinationDir));
            }

            // Copy all the files & replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourceDir, destinationDir), true);
            }
        }

        public static async Task DeleteSteamDepot()
        {
            var dialog = new ConfirmationDialog();
            dialog.ShowDialog();

            if (dialog.Result)
            {
                try
                {
                    string steamPath = null;
                    string[] possibleSteamPaths = new string[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                        "C:\\Steam",
                        "D:\\Steam"
                    };

                    // Find Steam installation
                    foreach (var path in possibleSteamPaths)
                    {
                        if (Directory.Exists(path))
                        {
                            steamPath = path;
                            break;
                        }
                    }

                    if (steamPath != null)
                    {
                        string depotPath = Path.Combine(steamPath, "steamapps", "content", "app_205710", "depot_205711");
                        if (Directory.Exists(depotPath))
                        {
                            await Task.Run(() => Directory.Delete(depotPath, true));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Steam depot cleanup error: {ex.Message}");
                    MessageBox.Show($"Error during cleanup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static async Task MonitorDownloadProgress(string expectedPath, Action<string> updateStatus)
        {
            bool isComplete = false;
            int remainingSeconds = 60;
            const double TOTAL_GB = 8.91;

            while (!isComplete && remainingSeconds > 0)
            {
                double currentSize = await Task.Run(() => CalculateDirectorySize(expectedPath));
                double downloadedGB = Math.Round(currentSize / (1024.0 * 1024.0 * 1024.0), 2);
                
                // Debug output
                Debug.WriteLine($"Raw Size: {currentSize}, Converted GB: {downloadedGB}");
                
                // Calculate percentage against hardcoded value
                double percentage = Math.Min(100.0, Math.Round((downloadedGB / TOTAL_GB) * 100.0, 1));

                // Use hardcoded value in display
                updateStatus($"Downloaded: {downloadedGB:F2} GB of {TOTAL_GB:F2} GB ({percentage}%)");

                if (downloadedGB >= TOTAL_GB)
                {
                    isComplete = true;
                }
                else
                {
                    if (percentage < 95.0)
                    {
                        updateStatus("Download paused, reconnecting...");
                    }
                    else
                    {
                        updateStatus($"Checking download completion... ({remainingSeconds} seconds remaining)");
                    }
                    
                    await Task.Delay(1000);
                    remainingSeconds--;
                }
            }
        }
    }
} 