using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace StswExpress
{
    public static class Log
    {
        private static readonly string LogFileName = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), $"log.log");
        private static readonly FileInfo LogFile = new FileInfo(LogFileName);
        public static long ArchiveFileSize = 1024 * 1024 * 5; /// 5 MB
        
        /// <summary>
        /// Write text to log file.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="addfunccallername">Add function name.</param>
        public static void Write(string text, bool addfunccallername = false)
        {
            try
            {
                /// add func name
                if (addfunccallername)
                {
                    var stackTrace = new StackTrace();
                    text += $" ({stackTrace.GetFrame(2).GetMethod().Name})";
                }

                /// make archive
                if (File.Exists(LogFileName))
                {
                    if (LogFile.Length > ArchiveFileSize)
                    {
                        File.Copy(LogFileName, LogFileName.Replace(".", $"_archive_{DateTime.Now:yyyyMMdd_hhmmss}."));
                        File.WriteAllText(LogFileName, string.Empty);
                    }
                }

                /// write log
                using var strWr = new StreamWriter(LogFileName, true);
                strWr.Write($"{DateTime.Now:yyyy-MM-dd | HH:mm:ss.fff} | {text}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
