using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StswExpress;
/// <summary>
/// Provides a simple way to write log messages to a file.
/// </summary>
public static class StswLog
{
    private static int _failureCount = 0;

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
    public static async Task<IEnumerable<StswLogItem?>> Import(DateTime dateFrom, DateTime dateTo)
    {
        var allLogs = new List<string>();

        for (DateTime i = dateFrom; i <= dateTo; i = i.AddDays(1))
        {
            var logFile = Path.Combine(Config.LogDirectoryPath, $"log_{i:yyyy-MM-dd}.log");
            if (!File.Exists(logFile))
                continue;

            using var sr = new StreamReader(logFile);
            var log = string.Empty;
            while (!sr.EndOfStream)
            {
                if (await sr.ReadLineAsync() is string l)
                {
                    if (DateTime.TryParse(l[..19], out var _))
                    {
                        allLogs.Add(log.TrimEnd(Environment.NewLine));
                        log = string.Empty;
                    }
                    log += l + Environment.NewLine;
                }
            }
            allLogs.Add(log.TrimEnd(Environment.NewLine));
        }

        return allLogs.Where(x => x.Length > 0).Select(log =>
        {
            var descBeginsAt = log.Length >= 24 && log[20] == '|' && log[24] == '|' ? 26 : 22;
            var type = (StswInfoType)Enum.Parse(typeof(StswInfoType), Enum.GetNames(typeof(StswInfoType)).First(x => x.StartsWith(descBeginsAt == 26 ? log[22..23] : "N")));
            return DateTime.TryParse(log[..19], out var date) ? new StswLogItem(type, log[descBeginsAt..]) { DateTime = date } : (StswLogItem?)null;
        }).ToList();
    }

    /// <summary>
    /// Writes a log entry to a file asynchronously in the directory specified by <see cref="Config.LogDirectoryPath"/>.
    /// Every log entry is written on a new line and starts with the current date and time, with a new file created each day.
    /// </summary>
    /// <param name="type">The type of log entry.</param>
    /// <param name="text">The text to log.</param>
    public static async Task WriteAsync(StswInfoType? type, string text)
    {
        if (Config.IsLoggingDisabled)
            return;

        if (!ShouldLog(type))
            return;

        /// CREATE DIRECTORY & CHECK ARCHIVE
        // if (Config.ArchiveWhenSizeOver != null && new FileInfo(Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log")).Length > Config.ArchiveWhenSizeOver)
        //     Archive(DateTime.Now);

        /// CREATE LOG
        try
        {
            var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {type?.ToString().FirstOrDefault()} | {text}";

            using var sw = new StreamWriter(Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true);
            await sw.WriteLineAsync(log);
            _failureCount = 0;
        }
        catch (Exception ex)
        {
            HandleLoggingFailure(ex);
        }
    }

    /// <summary>
    /// Writes a log entry to a file asynchronously in the directory specified by <see cref="Config.LogDirectoryPath"/>.
    /// This method is a shorthand for writing a simple text log entry without specifying a type.
    /// </summary>
    /// <param name="text">The text to log.</param>
    public static Task WriteAsync(string text) => WriteAsync(null, text);

    /// <summary>
    /// Writes a log entry to a file synchronously in the directory specified by <see cref="Config.LogDirectoryPath"/>.
    /// Every log entry is written on a new line and starts with the current date and time, with a new file created each day.
    /// </summary>
    /// <param name="type">The type of log entry.</param>
    /// <param name="text">The text to log.</param>
    public static void Write(StswInfoType? type, string text)
    {
        if (Config.IsLoggingDisabled)
            return;

        if (!ShouldLog(type))
            return;

        /// CREATE DIRECTORY & CHECK ARCHIVE
        // if (Config.ArchiveWhenSizeOver != null && new FileInfo(Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log")).Length > Config.ArchiveWhenSizeOver)
        //     Archive(DateTime.Now);

        /// CREATE LOG
        try
        {
            var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {type?.ToString().FirstOrDefault()} | {text}";

            using var sw = new StreamWriter(Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true);
            sw.WriteLine(log);
            _failureCount = 0;
        }
        catch (Exception ex)
        {
            HandleLoggingFailure(ex);
        }
    }

    /// <summary>
    /// Writes a log entry to a file synchronously in the directory specified by <see cref="Config.LogDirectoryPath"/>.
    /// This method is a shorthand for writing a simple text log entry without specifying a type.
    /// </summary>
    /// <param name="text">The text to log.</param>
    public static void Write(string text) => Write(null, text);

    /// <summary>
    /// Determines whether the log entry should be written based on the type of the log and the current configuration.
    /// </summary>
    /// <param name="type">The type of log entry.</param>
    /// <returns><see langword="true"/> if the log entry should be written; otherwise, <see langword="false"/>.</returns>
    private static bool ShouldLog(StswInfoType? type)
    {
        if (type == null)
            return true;

        if (Assembly.GetEntryAssembly()?.IsInDebug() == true)
            return type.Value.In(Config.LogTypes_DEBUG);
        else
            return type.Value.In(Config.LogTypes_RELEASE);
    }

    /// <summary>
    /// Handles failures that occur during the logging process. 
    /// Increments the failure count, and if the number of failures exceeds the configured maximum, logging is disabled.
    /// Additionally, a custom failure action can be invoked if configured.
    /// </summary>
    /// <param name="ex">The exception that occurred during the logging process.</param>
    private static void HandleLoggingFailure(Exception ex)
    {
        _failureCount++;
        if (_failureCount >= Config.MaxFailures)
            Config.IsLoggingDisabled = true;

        Config.OnLogFailure?.Invoke(ex);
    }
}
