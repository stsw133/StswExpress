using System.Globalization;
using System.IO.Compression;

namespace StswExpress.Commons;
/// <summary>
/// Provides a simple way to write and manage log messages, including support for automatic archiving and error handling.
/// </summary>
public static class StswLog
{
    private static int _failureCount = 0;

    static StswLog()
    {
        if (!string.IsNullOrEmpty(Config.ArchiveDirectoryPath) && !Directory.Exists(Config.ArchiveDirectoryPath))
            Directory.CreateDirectory(Config.ArchiveDirectoryPath);

        if (!string.IsNullOrEmpty(Config.LogDirectoryPath) && !Directory.Exists(Config.LogDirectoryPath))
            Directory.CreateDirectory(Config.LogDirectoryPath);

        AutoArchive();
    }

    /// <summary>
    /// Gets the current configuration settings for logging.
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
        string archivePath;
        if (Config.ArchiveFullMonth && dateFrom.Year == dateTo.Year && dateFrom.Month == dateTo.Month)
            archivePath = $"archive_{dateFrom:yyyy-MM}.zip";
        else if (dateFrom == dateTo)
            archivePath = $"archive_{dateFrom:yyyy-MM-dd}.zip";
        else
            archivePath = $"archive_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.zip";

        var fullArchivePath = Path.Combine(Config.ArchiveDirectoryPath, archivePath);
        if (File.Exists(fullArchivePath))
            return;

        using var archive = ZipFile.Open(Path.Combine(Config.ArchiveDirectoryPath, archivePath), ZipArchiveMode.Create);
        foreach (var file in Directory.GetFiles(Config.LogDirectoryPath).Where(x =>
                             DateTime.ParseExact(Path.GetFileNameWithoutExtension(x).TrimStart("log_"), "yyyy-MM-dd", CultureInfo.InvariantCulture)
                             .Between(dateFrom.Date, dateTo.Date)))
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
    /// Synchronously imports log entries from log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    /// <returns>An enumerable collection of <see cref="StswLogItem"/> objects representing the imported log entries.</returns>
    /// <remarks>
    /// This method blocks the calling thread while the log files are being read. 
    /// For non-blocking operations, consider using the asynchronous version <see cref="ImportListAsync"/>.
    /// </remarks>
    public static IEnumerable<StswLogItem> ImportList(DateTime dateFrom, DateTime dateTo) => ImportListAsync(dateFrom, dateTo).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously imports log entries from log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    /// <returns>A task representing the asynchronous operation, with a result of a collection of log entries.</returns>
    public static async Task<IEnumerable<StswLogItem>> ImportListAsync(DateTime dateFrom, DateTime dateTo)
    {
        var logItems = new List<StswLogItem>();

        for (DateTime date = dateFrom.Date; date <= dateTo.Date; date = date.AddDays(1))
        {
            var logFilePath = Path.Combine(Config.LogDirectoryPath, $"log_{date:yyyy-MM-dd}.log");
            if (!File.Exists(logFilePath))
                continue;

            var lines = await File.ReadAllLinesAsync(logFilePath);
            var currentLogItem = new List<string>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (DateTime.TryParse(line[..19], out var dateTime))
                {
                    if (currentLogItem.Count > 0)
                    {
                        var logItem = ParseLogEntry(currentLogItem);
                        if (logItem.HasValue)
                            logItems.Add(logItem.Value);
                        currentLogItem.Clear();
                    }
                }

                currentLogItem.Add(line);
            }

            if (currentLogItem.Count > 0)
            {
                var logItem = ParseLogEntry(currentLogItem);
                if (logItem.HasValue)
                    logItems.Add(logItem.Value);
            }
        }

        return logItems;
    }

    /// <summary>
    /// Writes a log entry to a file synchronously in the directory specified by <see cref="Config.LogDirectoryPath"/>.
    /// </summary>
    /// <param name="type">The type of the log entry.</param>
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
            var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(type ?? StswInfoType.None).ToString()[0]} | {text}";

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
    /// Writes a log entry to a file synchronously without specifying a log type.
    /// </summary>
    /// <param name="text">The text to log.</param>
    public static void Write(string text) => Write(null, text);


    /// <summary>
    /// Writes a log entry to a file asynchronously in the directory specified by <see cref="Config.LogDirectoryPath"/>.
    /// </summary>
    /// <param name="type">The type of the log entry.</param>
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
    /// Writes a log entry to a file asynchronously without specifying a log type.
    /// </summary>
    /// <param name="text">The text to log.</param>
    public static Task WriteAsync(string text) => WriteAsync(null, text);


    /// <summary>
    /// Handles logging failures by incrementing the failure count and disabling logging if the maximum number of failures is reached.
    /// Also invokes a custom failure action if one is configured.
    /// </summary>
    /// <param name="ex">The exception that occurred during the logging process.</param>
    private static void HandleLoggingFailure(Exception ex)
    {
        _failureCount++;
        if (_failureCount >= Config.MaxFailures)
            Config.IsLoggingDisabled = true;

        Config.OnLogFailure?.Invoke(ex);
    }

    /// <summary>
    /// Parses a single line from the log file and converts it into a <see cref="StswLogItem"/> object.
    /// </summary>
    /// <param name="line">The log entry line as a string.</param>
    /// <returns>
    /// A <see cref="StswLogItem"/> object if the line is valid; otherwise, <see langword="null"/> if the line is invalid or cannot be parsed.
    /// </returns>
    /// <remarks>
    /// This method assumes that the log line is formatted as "yyyy-MM-dd HH:mm:ss | T | Log text", where 'T' represents the first character of the log type.
    /// </remarks>
    private static StswLogItem? ParseLogEntry(List<string> logEntryLines)
    {
        if (logEntryLines.Count == 0)
            return null;

        var firstLine = logEntryLines[0];
        if (!DateTime.TryParse(firstLine[..19], out var date))
            return null;

        var typeChar = firstLine[22];
        var type = Enum.GetValues(typeof(StswInfoType))
                       .Cast<StswInfoType>()
                       .FirstOrDefault(t => t.ToString().StartsWith(typeChar.ToString(), StringComparison.OrdinalIgnoreCase));

        var textBuilder = new List<string> { firstLine.Length > 26 ? firstLine[26..] : string.Empty };
        textBuilder.AddRange(logEntryLines.Skip(1));
        var text = string.Join(Environment.NewLine, textBuilder);

        return new StswLogItem(type, text, date);
    }

    /// <summary>
    /// Determines whether the log entry should be written based on the type of the log and the current configuration.
    /// </summary>
    /// <param name="type">The type of log entry.</param>
    /// <returns><see langword="true"/> if the log entry should be written; otherwise, <see langword="false"/>.</returns>
    private static bool ShouldLog(StswInfoType? type)
    {
        if (type == null)
            return true;

        return StswFn.IsInDebug()
            ? type.Value.In(Config.LogTypes_DEBUG)
            : type.Value.In(Config.LogTypes_RELEASE);
    }
}
