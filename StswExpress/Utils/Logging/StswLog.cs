using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StswExpress;
/// <summary>
/// Provides a simple way to write log messages to a file.
/// </summary>
public static class StswLog
{
    static StswLog()
    {
        AutoArchive();
    }

    /// <summary>
    /// Specifies the configuration settings.
    /// </summary>
    public static StswLogConfig Config { get; } = new();

    /// <summary>
    /// Automatically archives log files based on the configuration settings.
    /// </summary>
    private static void AutoArchive()
    {
        if (new DirectoryInfo(Config.LogDirectoryPath).GetFileSystemInfos("*.log").OrderBy(x => x.CreationTime).FirstOrDefault() is not FileSystemInfo oldestFileInfo)
            return;

        var oldestFileDT = oldestFileInfo.CreationTime;
        var dateNow = DateTime.Now.Date;

        if (Config.ArchiveFullMonth && (oldestFileDT.Year != dateNow.Year || oldestFileDT.Month != dateNow.Month))
            foreach (var (year, month) in StswFn.GetUniqueMonthsFromRange(oldestFileDT, dateNow.AddMonths(-1)))
                Archive(new DateTime(year, month, 1), new DateTime(year, month, DateTime.DaysInMonth(year, month)));
        else if ((dateNow - oldestFileDT).TotalDays > Config.ArchiveWhenDaysOver)
            Archive(oldestFileDT, dateNow.AddDays(-Config.ArchiveUpToLastDays));
    }

    /// <summary>
    /// Archives log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    public static void Archive(DateTime dateFrom, DateTime dateTo)
    {
        if (!Directory.Exists(Config.ArchiveDirectoryPath))
            Directory.CreateDirectory(Config.ArchiveDirectoryPath);

        string archivePath;
        if (Config.ArchiveFullMonth && dateFrom.Year == dateTo.Year && dateFrom.Month == dateTo.Month)
            archivePath = $"archive_{dateFrom:yyyy-MM}.zip";
        else if (dateFrom == dateTo)
            archivePath = $"archive_{dateFrom:yyyy-MM-dd}.zip";
        else
            archivePath = $"archive_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.zip";

        using var archive = ZipFile.Open(Path.Combine(Config.ArchiveDirectoryPath, archivePath), ZipArchiveMode.Create);
        foreach (var file in Directory.GetFiles(Config.LogDirectoryPath).Where(x => DateTime.ParseExact(Path.GetFileNameWithoutExtension(x).TrimStart("log_"), "yyyy-MM-dd", CultureInfo.InvariantCulture).Between(dateFrom.Date, dateTo.Date)))
        {
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
            File.Delete(file);
        }
    }

    /// <summary>
    /// Archives log files for a single specified date.
    /// </summary>
    /// <param name="date">The date to archive.</param>
    public static void Archive(DateTime date) => Archive(date, date);

    /// <summary>
    /// Imports log entries from log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    /// <returns>A collection of log entries.</returns>
    public static IEnumerable<StswLogItem?> Import(DateTime dateFrom, DateTime dateTo)
    {
        /// load all lines from log files
        List<string> allLogs = [];

        for (DateTime i = dateFrom; i <= dateTo; i = i.AddDays(1))
        {
            var logFile = Path.Combine(Config.LogDirectoryPath, $"log_{i:yyyy-MM-dd}.log");
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
            if (DateTime.TryParse(log[..19], out var date))
                yield return new StswLogItem(type, log[descBeginsAt..]) { DateTime = date };
        }
    }

    /// <summary>
    /// Writes a log entry to a file in the directory specified by <see cref="LogDirectoryPath"/>.
    /// Every log entry is written in a new line and starts with current date and time with a new file created each day.
    /// </summary>
    /// <param name="type">The type of log entry.</param>
    /// <param name="text">The text to log.</param>
    public static void Write(StswInfoType? type, string text)
    {
        /// CONDITIONS
        if (type != null)
        {
            if (Assembly.GetEntryAssembly()?.IsInDebug() == true)
            {
                if (!type.Value.In(Config.LogTypes_DEBUG))
                    return;
            }
            else
            {
                if (!type.Value.In(Config.LogTypes_RELEASE))
                    return;
            }
        }

        /// CREATE DIRECTORY & CHECK ARCHIVE
        /*
        if (Config.ArchiveWhenSizeOver != null && new FileInfo(Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log")).Length > Config.ArchiveWhenSizeOver)
            Archive(DateTime.Now);
        */
        /// CREATE LOG
        var log = new StringBuilder(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        if (type != null)
            log.Append($" | {type.Value.ToString().First()}");
        log.Append($" | {text}");

        using var sw = new StreamWriter(Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true);
        sw.WriteLine(log);
    }

    /// <summary>
    /// Writes a log entry to a file in the directory specified by <see cref="Config.LogDirectoryPath"/>.
    /// Every log entry is written in a new line and starts with current date and time with a new file created each day.
    /// </summary>
    /// <param name="text">The text to log.</param>
    public static void Write(string text) => Write(null, text);
}
