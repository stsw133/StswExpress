using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace StswExpress;

/// <summary>
/// Provides a simple way to write log messages to a file.
/// </summary>
public static class StswLog
{
    /// <summary>
    /// Specifies the path to the directory where the log files will be saved.
    /// </summary>
    public static string DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    /// <summary>
    /// Writes a log entry to a file in the directory specified by <see cref="DirectoryPath"/>.
    /// Every log entry is written in a new line and starts with current date and time with a new file created each day.
    /// </summary>
    /// <param name="text">Text to log.</param>
    public static ObservableCollection<StswLogItem> Import(DateTime dateF, DateTime dateT)
    {
        var result = new List<StswLogItem>();

        if (!Directory.Exists(DirectoryPath))
            return result.ToObservableCollection();

        void ParseLog(StringBuilder logBuilder, int endOfDate)
        {
            var log = logBuilder.ToString().TrimEnd(Environment.NewLine);
            var dateTime = Convert.ToDateTime(log[..19]);
            StswLogType? type = null;
            if (log[endOfDate + 6] == '|')
                type = (StswLogType)Enum.Parse(typeof(StswLogType), Enum.GetNames(typeof(StswLogType)).First(x => x.StartsWith(log[endOfDate + 4])));
            var text = type == null ? log[(endOfDate + 4)..] : log[(endOfDate + 8)..];

            result.Add(new(type ?? StswLogType.None, text) { DateTime = dateTime });
            logBuilder.Clear();
        }
        for (DateTime i = dateF; i <= dateT; i = i.AddDays(1))
        {
            if (!File.Exists(Path.Combine(DirectoryPath, $"log_{i:yyyy-MM-dd}.log")))
                continue;

            using (var sr = new StreamReader(Path.Combine(DirectoryPath, $"log_{i:yyyy-MM-dd}.log")))
            {
                var logBuilder = new StringBuilder(sr.ReadLine());
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    if (line != null && DateTime.TryParse(line[..19], out var _))
                    {
                        var endOfDate = line[19] == '.' ? 22 : 18;

                        if (line[endOfDate + 2] == '|')
                            ParseLog(logBuilder, endOfDate);
                    }

                    logBuilder.AppendLine(line);
                }
                ParseLog(logBuilder, logBuilder.ToString()[19] == '.' ? 22 : 18);
            }
        }

        return result.ToObservableCollection();
    }

    /// <summary>
    /// Writes a log entry to a file in the directory specified by <see cref="DirectoryPath"/>.
    /// Every log entry is written in a new line and starts with current date and time with a new file created each day.
    /// </summary>
    /// <param name="text">Text to log.</param>
    public static void Write(StswLogType type, string text)
    {
        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        using (var sw = new StreamWriter(Path.Combine(DirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true))
            sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {type.ToString().First()} | {text}");
    }

    /// <summary>
    /// Writes a log entry to a file in the directory specified by <see cref="DirectoryPath"/>.
    /// Every log entry is written in a new line and starts with current date and time with a new file created each day.
    /// </summary>
    /// <param name="text">Text to log.</param>
    public static void Write(string text)
    {
        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        using (var sw = new StreamWriter(Path.Combine(DirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true))
            sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {text}");
    }
}
