using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace THJPatcher.Helpers
{
    public static class FileVerificationHelper
    {
        private static readonly string[] RequiredDlls = new[]
        {
            "mss32.dll",
            "xul.dll",
            "nspr4.dll",
            "steam_api.dll"
        };

        public static void VerifyAndCopyRequiredFiles(string steamDepotPath, string installPath, Action<string> updateStatus)
        {
            try
            {
                updateStatus("Verifying required DLL files...");
                
                foreach (string dllName in RequiredDlls)
                {
                    string targetPath = Path.Combine(installPath, dllName);
                    string sourcePath = Path.Combine(steamDepotPath, dllName);

                    if (!File.Exists(targetPath) && File.Exists(sourcePath))
                    {
                        updateStatus($"Copying required file: {dllName}");
                        File.Copy(sourcePath, targetPath, true);
                    }
                }

                // Verify all required files are present after copying
                string[] missingFiles = RequiredDlls
                    .Where(dll => !File.Exists(Path.Combine(installPath, dll)))
                    .ToArray();

                if (missingFiles.Any())
                {
                    string missingFilesList = string.Join(", ", missingFiles);
                    throw new FileNotFoundException(
                        $"Critical files are missing: {missingFilesList}. " +
                        "Please verify your Steam EverQuest installation and try again.");
                }

                updateStatus("All required files verified successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during file verification: {ex.Message}",
                    "File Verification Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                throw;
            }
        }
    }
} 