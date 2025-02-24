using System;
using System.Diagnostics;
using System.IO;
using System.Windows;


namespace THJPatcher.Launcher
{
    public class PlayGameHandler
    {
        public static void LaunchGame()
        {
            try
            {
                string parentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;

                // Set the path to EQ executable
                string gamePath = Path.Combine(parentDirectory, "eqgame.exe");

                // Check if EQ executable exists
                if (File.Exists(gamePath))
                {
                    // Start the EQ Game process
                    Process.Start(gamePath);
                    Application.Current.Shutdown();
                }
                else
                {
                    MessageBox.Show("Game launcher not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to start the game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
