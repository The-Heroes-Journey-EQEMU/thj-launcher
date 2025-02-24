using System;
using System.IO;
using THJPatcher.Helpers;

namespace THJPatcher.Helpers
{
    public class UIBackupHelper
    {
        // Define the default UI string content
        private static string DefaultUIContent = "[Default UI Content]";

        // Method to handle UI file backup
        public static void BackupValidUIFiles(FileInfo[] files, Action<string> updateStatusCallback)
        {
            bool backupCreated = false;

            foreach (var file in files)
            {
                if (file.Length < 10240)
                    continue;

                string bakFile = file.FullName + ".bak";

                // Force overwrite the existing backup file
                if (File.Exists(bakFile))
                {
                    File.Delete(bakFile);
                }

                File.Copy(file.FullName, bakFile);
                updateStatusCallback($"Backup created for {file.Name}.");
                backupCreated = true;
            }

            if (!backupCreated)
            {
                updateStatusCallback("No valid UI files found to back up.");
            }
        }

        public static void HandleCorruptedUIFile(FileInfo file, Action<string> updateStatusCallback)
        {
            string bakFile = file.FullName + ".bak";

            // Check if a backup exists
            if (File.Exists(bakFile))
            {
                updateStatusCallback($"Backup for {file.Name} exists. Restoring...");
                File.Copy(bakFile, file.FullName, true);
                return;
            }

            // If no backup exists, restore to default UI
            updateStatusCallback($"No backup for {file.Name}. Creating default backup...");
            File.WriteAllText(file.FullName, DefaultUIContent);
        }
    }
}

