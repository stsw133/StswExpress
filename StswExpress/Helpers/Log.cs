using System;
using System.IO;
using System.Reflection;

namespace StswExpress
{
    public static class Log
    {
        private static readonly string LogFileName = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath), $"log.log");
        private static readonly FileInfo LogFile = new FileInfo(LogFileName);
        private static readonly long ArchiveFileSize = 1024 * 1024 * 5;

        /// Write text to log file.
        public static void Write(string text)
        {
            if (File.Exists(LogFileName) && LogFile.Length > ArchiveFileSize)
            {
                File.Copy(LogFileName, LogFileName.Replace(".", $"_archive_{DateTime.Now:yyyyMMdd_hhmmss}."));
                File.WriteAllText(LogFileName, string.Empty);
            }

            using var strWr = new StreamWriter(LogFileName, true);
            strWr.Write($"{DateTime.Now:yyyy-MM-dd  HH:mm:ss.fff} | {text}{Environment.NewLine}");
        }
    }
}
