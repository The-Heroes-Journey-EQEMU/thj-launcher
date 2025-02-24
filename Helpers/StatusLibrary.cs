using System;
using System.IO;

namespace THJPatcher.Helpers
{
    public static class StatusLibrary
    {
        private static string logFile = "patcher_log.txt";

        public static void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timestamp}] {message}";
            
            Console.WriteLine(logMessage);
            
            try
            {
                File.AppendAllText(logFile, logMessage + Environment.NewLine);
            }
            catch
            {
            }
        }

        public static void ClearLog()
        {
            try
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
            catch
            {
            }
        }
    }
}