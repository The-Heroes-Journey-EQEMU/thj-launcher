using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace THJPatcher.Helpers
{
    public static class PatchHelper
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "THJPatcher"
        );
        private static readonly string localFilePath = Path.Combine(AppDataPath, "filelist.yml");
        private static readonly string patcherUrl = "https://raw.githubusercontent.com/BND10706/thj-patcher-files/main/filelist.yml";
        private static string currentVersion = "--";

        static PatchHelper()
        {
            // Ensure directory exists
            Directory.CreateDirectory(AppDataPath);
        }

        public static string CurrentVersion 
        { 
            get { return currentVersion; }
            private set 
            { 
                currentVersion = value;
                OnVersionChanged?.Invoke(value);
            }
        }

        public static event Action<string> OnVersionChanged;

        public static string GetCurrentVersion()
        {
            if (File.Exists(localFilePath))
            {
                try
                {
                    string localContent = File.ReadAllText(localFilePath);
                    var filelist = ParseFileList(localContent);
                    return filelist?.Version ?? "--";
                }
                catch
                {
                    return "--";
                }
            }
            return "--";
        }

        // Method to check the patch and display progress using a callback
        public static async Task<bool> CheckAndDisplayPatchVersion(Action<string> updateStatusCallback)
        {
            try
            {
                updateStatusCallback("Checking for available patches...");

                // Get local version first
                string localVersion = GetCurrentVersion();
                if (!string.IsNullOrEmpty(localVersion))
                {
                    CurrentVersion = localVersion;
                }

                // Get remote version
                var remoteFileList = await GetRemoteFileList();
                if (remoteFileList != null)
                {
                    // Check if any files need updating
                    bool needsUpdate = await CheckAndUpdateFiles(remoteFileList, updateStatusCallback);
                    
                    if (!needsUpdate)
                    {
                        updateStatusCallback("All files are up to date.");
                        return false;
                    }
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                updateStatusCallback($"An error occurred while fetching the patch: {ex.Message}");
                throw;
            }
        }

        // Method to fetch and return the patch version
        private static async Task<string> GetPatchVersion(Action<string> updateStatusCallback)
        {
            try
            {
                // Step 1: Check if the local file exists, read it
                if (File.Exists(localFilePath))
                {
                    Console.WriteLine($"Local {localFilePath} found, reading version...");
                    string localContent = await File.ReadAllTextAsync(localFilePath);
                    var filelist = ParseFileList(localContent);

                    if (filelist != null)
                    {
                        Console.WriteLine($"Parsed local filelist, patch version: {filelist.Version}");
                        CurrentVersion = filelist.Version;
                        updateStatusCallback($"Local patch version: {filelist.Version}");
                        return filelist.Version;
                    }
                    else
                    {
                        Console.WriteLine("Local file could not be parsed. Fetching from GitHub...");
                    }
                }

                // Step 2: If no local file or if it could not be parsed, download from GitHub
                updateStatusCallback($"Attempting to download filelist.yml from: {patcherUrl}");
                var response = await client.GetStringAsync(patcherUrl);

                // Log the raw content of the YAML file
                Console.WriteLine("Received filelist.yml content:");
                Console.WriteLine(response);

                // Parse the YAML file to extract version
                var filelistFromGitHub = ParseFileList(response);

                if (filelistFromGitHub == null)
                {
                    Console.WriteLine("Failed to parse the GitHub filelist.");
                    updateStatusCallback("Failed to parse the patch file.");
                    return null;
                }

                // Save the fetched file locally
                await File.WriteAllTextAsync(localFilePath, response);

                Console.WriteLine($"Parsed GitHub filelist, patch version: {filelistFromGitHub.Version}");
                updateStatusCallback($"Fetched patch version: {filelistFromGitHub.Version}");

                if (filelistFromGitHub != null)
                {
                    CurrentVersion = filelistFromGitHub.Version;
                }

                return filelistFromGitHub?.Version;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching patch version: {ex.Message}");
                updateStatusCallback($"Error fetching patch: {ex.Message}");
                return null;
            }
        }

        // Parsing the filelist from the YAML content
        private static FileList ParseFileList(string yamlContent)
        {
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                var filelist = deserializer.Deserialize<FileList>(yamlContent);
                return filelist;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing filelist YAML: {ex.Message}");
                return null;
            }
        }

        private static async Task<bool> CheckAndUpdateFiles(FileList remoteFileList, Action<string> updateStatusCallback)
        {
            bool updatesNeeded = false;

            foreach (var fileEntry in remoteFileList.Downloads)
            {
                string localPath = Path.Combine(AppDataPath, fileEntry.Name);
                
                if (!File.Exists(localPath))
                {
                    updatesNeeded = true;
                    updateStatusCallback($"Missing file: {localPath}");
                    continue;
                }

                string localMd5 = await CalculateFileMd5(localPath);

                if (localMd5 != fileEntry.Md5)
                {
                    updatesNeeded = true;
                    updateStatusCallback($"File needs update: {fileEntry.Name}");
                }
            }

            return updatesNeeded;
        }

        // Testing
        private static async Task DownloadAndUpdateFile(string filePath, string remoteUrl, Action<string> updateStatusCallback)
        {
            try
            {
                updateStatusCallback($"Downloading: {filePath}");
                string content = await client.GetStringAsync(remoteUrl);
                content = "Hello World 2!";
                
                await File.WriteAllTextAsync(filePath, content, new System.Text.UTF8Encoding(false));
                updateStatusCallback($"Updated: {filePath}");
            }
            catch (Exception ex)
            {
                updateStatusCallback($"Error updating {filePath}: {ex.Message}");
                throw;
            }
        }

        private static async Task<string> CalculateFileMd5(string filePath)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating MD5 for {filePath}: {ex.Message}");
                return null;
            }
        }

        public static async Task DownloadAndApplyUpdates(Action<string> updateStatusCallback)
        {
            try
            {
                var fileList = await GetRemoteFileList();
                
                // Handle deletes first
                if (fileList.Deletes != null)
                {
                    updateStatusCallback($"Processing {fileList.Deletes.Count} file deletion(s)...");
                    foreach (var deleteEntry in fileList.Deletes)
                    {
                        string localPath = Path.Combine(AppDataPath, deleteEntry.Name);
                        if (File.Exists(localPath))
                        {
                            try
                            {
                                File.Delete(localPath);
                                updateStatusCallback($"Removed: {deleteEntry.Name}");
                            }
                            catch (Exception ex)
                            {
                                updateStatusCallback($"Warning: Could not delete {deleteEntry.Name}: {ex.Message}");
                            }
                        }
                    }
                }

                // Then handle updates
                if (fileList.Downloads != null)
                {
                    foreach (var fileEntry in fileList.Downloads)
                    {
                        string localPath = Path.Combine(AppDataPath, fileEntry.Name);
                        string remoteUrl = fileList.DownloadPrefix + fileEntry.Name;
                        
                        // Normalize paths for comparison
                        string normalizedLocalPath = localPath.Replace('\\', '/');
                        string normalizedRemotePath = fileEntry.Name.Replace('\\', '/');
                        
                        if (!File.Exists(localPath))
                        {
                            updateStatusCallback($"Missing file: {normalizedRemotePath}");
                            await DownloadAndUpdateFile(localPath, remoteUrl, updateStatusCallback);
                            continue;
                        }

                        string localMd5 = await CalculateFileMd5(localPath);
                        if (localMd5 == null)
                        {
                            updateStatusCallback($"Warning: Could not calculate MD5 for {normalizedRemotePath}, skipping...");
                            continue;
                        }

                        if (localMd5 != fileEntry.Md5)
                        {
                            updateStatusCallback($"Modified file detected: {normalizedRemotePath}");
                            updateStatusCallback($"Expected MD5: {fileEntry.Md5}, Got: {localMd5}");
                            await DownloadAndUpdateFile(localPath, remoteUrl, updateStatusCallback);
                        }
                    }
                }

                // Update the local filelist
                if (fileList != null)
                {
                    var serializer = new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    string yamlContent = serializer.Serialize(fileList);
                    await File.WriteAllTextAsync(localFilePath, yamlContent);
                    CurrentVersion = fileList.Version;
                }

                updateStatusCallback($"Up to date with patch {fileList?.Version ?? "unknown"}.");
                updateStatusCallback("Patch complete! Ready to play!");
            }
            catch (Exception ex)
            {
                updateStatusCallback($"Error during patching: {ex.Message}");
                throw;
            }
        }

        // Add this utility method
        public static string GenerateMd5ForContent(string content)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                // Ensure consistent line endings and encoding
                content = content.Replace("\r\n", "\n").Replace("\r", "\n");
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private static async Task<FileList> GetRemoteFileList()
        {
            try
            {
                var response = await client.GetStringAsync(patcherUrl);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                return deserializer.Deserialize<FileList>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching remote file list: {ex.Message}");
                throw;
            }
        }
    }

    public class FileEntry
    {
        public string Name { get; set; }
        public string Md5 { get; set; }
        public string Date { get; set; }
        public long Size { get; set; }
    }

    public class FileList
    {
        public string Version { get; set; }
        public List<FileDelete> Deletes { get; set; }
        public string DownloadPrefix { get; set; }
        public List<FileEntry> Downloads { get; set; }
    }

    public class FileDelete
    {
        public string Name { get; set; }
    }
}
