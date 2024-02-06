using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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
        var result = new ObservableCollection<StswLogItem>();

        if (!Directory.Exists(DirectoryPath))
            return result;

        /// load all lines from log files
        List<string> allLogs = new();

        for (DateTime i = dateF; i <= dateT; i = i.AddDays(1))
        {
            var logFile = Path.Combine(DirectoryPath, $"log_{i:yyyy-MM-dd}.log");
            if (!File.Exists(logFile))
                continue;

            var log = string.Empty;

            using var sr = new StreamReader(logFile);
            while (!sr.EndOfStream)
                if (sr.ReadLine() is string l)
                {
                    if (DateTime.TryParse(l[..19], out var _))
                    {
                        allLogs.Add(log.TrimEnd(Environment.NewLine));
                        log = string.Empty;
                    }
                    log += l + Environment.NewLine;
                }
            allLogs.Add(log.TrimEnd(Environment.NewLine));
        }

        /// check every line so log can be added to list
        foreach (var log in allLogs.Where(x => x.Length > 0))
        {
            var descBeginsAt = log.Length >= 24 && log[20] == '|' && log[24] == '|' ? 26 : 22;
            var type = (StswInfoType)Enum.Parse(typeof(StswInfoType), Enum.GetNames(typeof(StswInfoType)).First(x => x.StartsWith(descBeginsAt == 26 ? log[22..23] : "N")));
            DateTime.TryParse(log[..19], out var date);
            result.Add(new StswLogItem(type, log[descBeginsAt..]) { DateTime = date });
        }

        return result;
    }

    /// <summary>
    /// Writes a log entry to a file in the directory specified by <see cref="DirectoryPath"/>.
    /// Every log entry is written in a new line and starts with current date and time with a new file created each day.
    /// </summary>
    /// <param name="text">Text to log.</param>
    public static void Write(StswInfoType type, string text)
    {
        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        using var sw = new StreamWriter(Path.Combine(DirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true);
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

        using var sw = new StreamWriter(Path.Combine(DirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true);
        sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {text}");
    }
}

/// <summary>
/// 
/// </summary>
public class StswLogItem
{
    public StswLogItem(StswInfoType type, string description)
    {
        Type = type;
        Description = description;
    }

    /// <summary>
    /// 
    /// </summary>
    public StswInfoType Type { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime DateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 
    /// </summary>
    public string? Description { get; set; }
}
