using System;
using System.IO;
using System.Windows;
using THJPatcher.Helpers;
using System.Threading.Tasks;

namespace THJPatcher.Launcher
{
    public class FileChecker
    {
        public static async void CheckForUpdates(Action<string> updateStatusCallback, Action enablePlayButtonCallback)
        {
            try
            {
                updateStatusCallback("Checking for updates...");

                // Check for patch and download/apply if needed
                bool updatesAvailable = await PatchHelper.CheckAndDisplayPatchVersion(updateStatusCallback);
                
                if (updatesAvailable)
                {
                    updateStatusCallback("Updates found. Downloading and applying...");
                    await PatchHelper.DownloadAndApplyUpdates(updateStatusCallback);
                    
                    // Update version after applying updates
                    updateStatusCallback($"Updated to version: {PatchHelper.CurrentVersion}");
                }
                else
                {
                    updateStatusCallback("No updates needed.");
                }

                // Enable play button
                enablePlayButtonCallback();
            }
            catch (Exception err)
            {
                MessageBox.Show($"An error occurred while checking for updates: {err.Message}", 
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public static void LaunchGame(Action<string> updateStatusCallback)
        {
            try
            {
                // Step 3: Backup UI files before launching the game
                updateStatusCallback("Backing up UI files...");

                string eqFolderPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;
                var di = new DirectoryInfo(eqFolderPath);
                var files = di.GetFiles("UI*_thj.ini");

                if (files.Length == 0)
                {
                    updateStatusCallback("No UI files found.");
                    return;
                }

                // Use UIBackupHelper to backup the UI files
                UIBackupHelper.BackupValidUIFiles(files, updateStatusCallback);
                StatusLibrary.Log("THJ UI backup complete.");
                updateStatusCallback("UI files backup complete.");

                // Step 4: Launch the game
                updateStatusCallback("Launching game...");
                
                // Todo: Launch the game
            }
            catch (Exception err)
            {
                MessageBox.Show($"An error occurred while trying to launch the game: {err.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
