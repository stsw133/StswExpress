using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace StswExpress
{
    public static class Log
    {
        private static readonly string LogFileDirectory = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath), "logs");

        /// Write text to log file.
        public static void Write(string text)
        {
            using (var sw = new StreamWriter(Path.Combine(LogFileDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true, Encoding.GetEncoding("Windows-1250")))
                sw.WriteLine($"{DateTime.Now:yyyy-MM-dd  HH:mm:ss.fff} | {text}");
        }
    }
}
