using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;

namespace StswExpress.Commons;
/// <summary>
/// Provides a simple way to write and manage log messages, including support for automatic archiving and error handling.
/// </summary>
[StswInfo(null)]
public static class StswLog
{
    private static readonly Dictionary<Guid, Dictionary<StswInfoType, int>> _logCounters = [];
    private static readonly SemaphoreSlim _logSemaphore = new(1, 1);
    private static int _failureCount = 0;

    static StswLog()
    {
        if (!string.IsNullOrEmpty(Config.Archive.ArchiveDirectoryPath) && !Directory.Exists(Config.Archive.ArchiveDirectoryPath))
            Directory.CreateDirectory(Config.Archive.ArchiveDirectoryPath);

        if (!string.IsNullOrEmpty(Config.LogDirectoryPath) && !Directory.Exists(Config.LogDirectoryPath))
            Directory.CreateDirectory(Config.LogDirectoryPath);

        AutoArchive();
    }

    /// <summary>
    /// Gets the current configuration settings for logging.
    /// </summary>
    public static StswLogConfig Config { get; } = new();

    #region Archive
    /// <summary>
    /// Automatically archives log files based on the configuration settings.
    /// </summary>
    [StswInfo("0.9.0")]
    private static void AutoArchive()
    {
        var dir = new DirectoryInfo(Config.LogDirectoryPath);
        var oldestFileInfo = dir.GetFileSystemInfos("log_*.log").OrderBy(x => x.CreationTime).FirstOrDefault();
        if (oldestFileInfo == null)
            return;

        var oldestFileDT = oldestFileInfo.CreationTime;
        var dateNow = DateTime.Now.Date;

        if (Config.Archive.ArchiveFullMonth && !oldestFileDT.IsSameYearAndMonth(dateNow))
            foreach (var (year, month) in StswFn.GetUniqueMonthsFromRange(oldestFileDT, dateNow.AddMonths(-1)))
                Archive(new DateTime(year, month, 1), new DateTime(year, month, DateTime.DaysInMonth(year, month)));
        else if ((dateNow - oldestFileDT).TotalDays > Config.Archive.ArchiveWhenDaysOver)
            Archive(oldestFileDT, dateNow.AddDays(-Config.Archive.ArchiveUpToLastDays));

        DeleteOldArchives();
    }

    /// <summary>
    /// Archives log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    [StswInfo("0.9.0")]
    public static void Archive(DateTime dateFrom, DateTime dateTo)
    {
        _logSemaphore.Wait();
        try
        {
            string archiveName;
            if (Config.Archive.ArchiveFullMonth && dateFrom.IsSameYearAndMonth(dateTo))
                archiveName = $"archive_{dateFrom:yyyy-MM}.zip";
            else if (dateFrom == dateTo)
                archiveName = $"archive_{dateFrom:yyyy-MM-dd}.zip";
            else
                archiveName = $"archive_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.zip";

            var fullArchivePath = Path.Combine(Config.Archive.ArchiveDirectoryPath, archiveName);
            //if (File.Exists(fullArchivePath))
            //    return;

            using var archive = ZipFile.Open(fullArchivePath, ZipArchiveMode.Update);
            foreach (var filePath in Directory.GetFiles(Config.LogDirectoryPath, "log_*.log"))
            {
                if (!TryGetDateFromFilename(filePath, out var fileDate))
                    continue;

                if (fileDate.Date.Between(dateFrom.Date, dateTo.Date))
                {
                    AddFileToZipWithPossibleRename(archive, filePath);
                    File.Delete(filePath);
                }
            }
        }
        finally
        {
            _logSemaphore.Release();
        }
    }

    /// <summary>
    /// Archives log files for a single specified date.
    /// </summary>
    /// <param name="date">The date to archive.</param>
    [StswInfo("0.9.0")]
    public static void Archive(DateTime date) => Archive(date, date);

    /// <summary>
    /// Deletes log archives that are older than the specified number of days.
    /// </summary>
    [StswInfo("0.9.0")]
    private static void DeleteOldArchives()
    {
        _logSemaphore.Wait();
        try
        {
            if (!Config.Archive.DeleteArchivesOlderThanDays.HasValue || Config.Archive.DeleteArchivesOlderThanDays.Value <= 0)
                return;

            int limitDays = Config.Archive.DeleteArchivesOlderThanDays.Value;
            var thresholdDate = DateTime.Now.Date.AddDays(-limitDays);

            foreach (var zipPath in Directory.GetFiles(Config.Archive.ArchiveDirectoryPath, "archive_*.zip"))
            {
                var fileName = Path.GetFileNameWithoutExtension(zipPath);

                if (TryGetArchiveDateRange(fileName, out var df, out var dt))
                {
                    if (dt.Date < thresholdDate)
                    {
                        try
                        {
                            File.Delete(zipPath);
                        }
                        catch (Exception ex)
                        {
                            Config.OnLogFailure?.Invoke(ex);
                        }
                    }
                }
            }
        }
        finally
        {
            _logSemaphore.Release();
        }
    }

    /// <summary>
    /// Adds a file to a zip archive, renaming it if a file with the same name already exists in the archive.
    /// </summary>
    /// <param name="zip"></param>
    /// <param name="sourceFilePath"></param>
    [StswInfo("0.9.0")]
    private static void AddFileToZipWithPossibleRename(ZipArchive zip, string sourceFilePath)
    {
        var baseFileName = Path.GetFileName(sourceFilePath); // log_2025-05-30.log
        var datePart = baseFileName.Substring(4, 10);        // 2025-05-30
        var extension = Path.GetExtension(baseFileName);     // .log
        var baseEntryName = $"log_{datePart}.log";

        var existingBaseEntry = zip.Entries.FirstOrDefault(e =>
            e.FullName.Equals(baseEntryName, StringComparison.OrdinalIgnoreCase));

        if (existingBaseEntry == null)
        {
            zip.CreateEntryFromFile(sourceFilePath, baseEntryName);
        }
        else
        {
            var timestamp = DateTime.Now.ToString("HH-mm-ss");
            var renamedName = $"log_{datePart}_{timestamp}{extension}";
            int counter = 1;

            while (zip.Entries.Any(e => e.FullName.Equals(renamedName, StringComparison.OrdinalIgnoreCase)))
                renamedName = $"log_{datePart}_{timestamp}_{counter++}{extension}";

            RenameZipArchiveEntry(zip, existingBaseEntry, renamedName);
            zip.CreateEntryFromFile(sourceFilePath, baseEntryName);
        }
    }

    /// <summary>
    /// Archives a single log file based on its size.
    /// </summary>
    /// <param name="logFile"></param>
    [StswInfo("0.9.0")]
    private static void ArchiveSingleLogBySize(FileInfo logFile)
    {
        string datePart = logFile.Name.Substring(4, 10);
        string archiveFileName = $"archive_{datePart}.zip";

        var archivePath = Path.Combine(Config.Archive.ArchiveDirectoryPath, archiveFileName);
        using var zip = ZipFile.Open(archivePath, ZipArchiveMode.Update);

        AddFileToZipWithPossibleRename(zip, logFile.FullName);

        logFile.Delete();
    }

    /// <summary>
    /// Forces the archiving of the current log file if the size exceeds the threshold specified in <see cref="Config.ArchiveWhenSizeOver"/>.
    /// </summary>
    [StswInfo("0.9.0")]
    private static void ForceSizeArchiveIfNeeded()
    {
        if (Config.Archive.ArchiveWhenSizeOver == null)
            return;

        var path = GetDailyLogFilePath();
        var fi = new FileInfo(path);
        if (!fi.Exists)
            return;

        if (fi.Length > Config.Archive.ArchiveWhenSizeOver.Value)
            ArchiveSingleLogBySize(fi);
    }

    /// <summary>
    /// Renames a <see cref="ZipArchiveEntry"/> in a <see cref="ZipArchive"/> to a new name.
    /// </summary>
    /// <param name="zip"></param>
    /// <param name="entry"></param>
    /// <param name="newName"></param>
    [StswInfo("0.9.0")]
    private static void RenameZipArchiveEntry(ZipArchive zip, ZipArchiveEntry entry, string newName)
    {
        if (zip.Entries.Any(e => e.FullName.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            newName = $"{Path.GetFileNameWithoutExtension(newName)}_{DateTime.Now:fff}.log";

        var newEntry = zip.CreateEntry(newName, CompressionLevel.Optimal);

        using (var oldStream = entry.Open())
        using (var newStream = newEntry.Open())
        {
            oldStream.CopyTo(newStream);
        }

        entry.Delete();
    }

    /// <summary>
    /// Tries to extract a date range from an archive name.
    /// </summary>
    /// <param name="archiveName"></param>
    /// <param name="dateFrom"></param>
    /// <param name="dateTo"></param>
    /// <returns></returns>
    [StswInfo("0.9.0")]
    private static bool TryGetArchiveDateRange(string archiveName, out DateTime dateFrom, out DateTime dateTo)
    {
        dateFrom = default;
        dateTo = default;

        if (!archiveName.StartsWith("archive_"))
            return false;

        var rest = archiveName["archive_".Length..];

        var parts = rest.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            if (TryParseYearMonth(parts[0], out var ymFrom))
            {
                dateFrom = new DateTime(ymFrom.Year, ymFrom.Month, 1);
                dateTo = dateFrom.AddMonths(1).AddDays(-1);
                return true;
            }
            else if (DateTime.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var singleDate))
            {
                dateFrom = singleDate.Date;
                dateTo = singleDate.Date;
                return true;
            }
            return false;
        }
        else if (parts.Length == 2)
        {
            if (TryParseYearMonth(parts[0], out var ym1) && TryParseYearMonth(parts[1], out var ym2))
            {
                dateFrom = new DateTime(ym1.Year, ym1.Month, 1);
                dateTo = new DateTime(ym2.Year, ym2.Month, 1).AddMonths(1).AddDays(-1);
                return true;
            }
            else
            {
                var success1 = DateTime.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dd1);
                var success2 = DateTime.TryParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dd2);

                if (success1 && success2)
                {
                    dateFrom = dd1.Date < dd2.Date ? dd1.Date : dd2.Date;
                    dateTo = dd1.Date > dd2.Date ? dd1.Date : dd2.Date;
                    return true;
                }
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to extract a date from a log file name.
    /// </summary>
    /// <param name="filePath">The path of the log file.</param>
    /// <param name="dt">The extracted date if successful.</param>
    /// <returns><see langword="true"/> if the date was successfully extracted; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetDateFromFilename(string filePath, out DateTime dt)
    {
        dt = default;
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        if (!fileName.StartsWith("log_"))
            return false;

        if (fileName.Length < 14)
            return false;

        var datePart = fileName.Substring(4, 10);
        return DateTime.TryParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
    }

    /// <summary>
    /// Tries to parse a string in the format "yyyy-MM" into a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="yearMonth">The parsed <see cref="DateTime"/> representing the first day of the month.</param>
    /// <returns><see langword="true"/> if the parsing was successful; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseYearMonth(string s, out DateTime yearMonth)
    {
        yearMonth = default;
        if (DateTime.TryParseExact(s, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            yearMonth = new DateTime(dt.Year, dt.Month, 1);
            return true;
        }
        return false;
    }
    #endregion

    #region Import
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
    public static IEnumerable<StswLogItem> ImportList(DateTime dateFrom, DateTime dateTo)
    {
        var logItems = new List<StswLogItem>();

        foreach (var filePath in GetFilesInRange(dateFrom, dateTo))
        {
            if (!File.Exists(filePath))
                continue;

            using var reader = new StreamReader(filePath);
            var currentBlock = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (IsNewLogEntryLine(line, out _))
                {
                    TryAddLogBlock(currentBlock, logItems);
                    currentBlock.Clear();
                }

                currentBlock.Add(line);
            }

            TryAddLogBlock(currentBlock, logItems);
        }

        return logItems;
    }

    /// <summary>
    /// Asynchronously imports log entries from log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    /// <returns>A task representing the asynchronous operation, with a result of a collection of log entries.</returns>
    public static async Task<IEnumerable<StswLogItem>> ImportListAsync(DateTime dateFrom, DateTime dateTo)
    {
        var logItems = new List<StswLogItem>();

        foreach (var filePath in GetFilesInRange(dateFrom, dateTo))
        {
            if (!File.Exists(filePath))
                continue;

            using var reader = new StreamReader(filePath);
            var currentBlock = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (IsNewLogEntryLine(line, out _))
                {
                    TryAddLogBlock(currentBlock, logItems);
                    currentBlock.Clear();
                }

                currentBlock.Add(line);
            }

            TryAddLogBlock(currentBlock, logItems);
        }

        return logItems;
    }

    /// <summary>
    /// Gets a list of log files within the specified date range.
    /// </summary>
    /// <param name="from">The start date of the range.</param>
    /// <param name="to">The end date of the range.</param>
    /// <returns>The list of file paths that match the date range.</returns>
    [StswInfo("0.20.0")]
    private static IEnumerable<string> GetFilesInRange(DateTime from, DateTime to)
        => Directory
            .GetFiles(Config.LogDirectoryPath, "log_*.log")
            .Where(path =>
                TryGetDateFromFilename(path, out var fileDate) &&
                fileDate.Date >= from.Date && fileDate.Date <= to.Date);

    /// <summary>
    /// Determines whether a line in the log file is the start of a new log entry.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <param name="date">The parsed date from the line if it is a new log entry.</param>
    /// <returns><see langword="true"/> if the line is a new log entry; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.20.0")]
    private static bool IsNewLogEntryLine(string line, out DateTime date)
    {
        date = default;
        return line.Length >= 19 && DateTime.TryParse(line[..19], out date);
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
        if (logEntryLines.Count == 0 || !IsNewLogEntryLine(logEntryLines[0], out var date))
            return null;

        var typeChar = logEntryLines[0].Length > 22 ? logEntryLines[0][22] : (char?)null;
        var type = Enum.GetValues(typeof(StswInfoType))
                       .Cast<StswInfoType>()
                       .FirstOrDefault(t =>
                            t.ToString().StartsWith(typeChar?.ToString() ?? "",
                                                    StringComparison.OrdinalIgnoreCase));

        var textList = new List<string>
        {
            logEntryLines[0].Length > 26 ? logEntryLines[0][26..] : string.Empty
        };
        textList.AddRange(logEntryLines.Skip(1));

        var text = string.Join(Environment.NewLine, textList);
        return new StswLogItem(type, text, date);
    }

    /// <summary>
    /// Tries to add a parsed log block to the output list.
    /// </summary>
    /// <param name="currentBlock">The current block of log lines to parse.</param>
    /// <param name="output">The output list to which the parsed log item will be added.</param>
    [StswInfo("0.20.0")]
    private static void TryAddLogBlock(List<string> currentBlock, List<StswLogItem> output)
    {
        if (currentBlock.Count == 0)
            return;

        var parsed = ParseLogEntry(currentBlock);
        if (parsed.HasValue)
            output.Add(parsed.Value);
    }
    #endregion

    #region Write
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

        _ = WriteInternal(type, text);
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
    [StswInfo("0.9.3")]
    public static Task WriteAsync(StswInfoType? type, string text)
    {
        if (Config.IsLoggingDisabled)
            return Task.CompletedTask;

        if (!ShouldLog(type))
            return Task.CompletedTask;

        /// CREATE LOG
        return WriteInternal(type, text);
    }

    /// <summary>
    /// Writes a log entry to a file asynchronously without specifying a log type.
    /// </summary>
    /// <param name="text">The text to log.</param>
    [StswInfo("0.9.3")]
    public static Task WriteAsync(string text) => WriteAsync(null, text);

    /// <summary>
    /// Writes an exception to the log file, including its type, message, stack trace, and any inner exceptions.
    /// </summary>
    /// <param name="ex">Exception to log</param>
    /// <param name="type">Optional type of the log entry. If not specified, defaults to <see cref="StswInfoType.Error"/>.</param>
    /// <param name="context">Optional context for the log entry, which can provide additional information about where the exception occurred.</param>
    [StswInfo("0.18.0")]
    public static void WriteException(Exception ex, StswInfoType? type = StswInfoType.Error, string? context = null)
    {
        if (Config.IsLoggingDisabled)
            return;

        if (!ShouldLog(type))
            return;

        /// CREATE LOG
        try
        {
            var msg = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(context))
                msg.AppendLine($"[{context}]");

            void AppendException(Exception e, int level)
            {
                var prefix = new string('>', level);
                msg.AppendLine($"{prefix} {e.GetType().Name}: {e.Message}");
                msg.AppendLine($"{prefix} {e.StackTrace}");
                if (e.InnerException != null)
                    AppendException(e.InnerException, level + 1);
            }

            AppendException(ex, 0);
            Write(type, msg.ToString());
            _failureCount = 0;
        }
        catch (Exception ex2)
        {
            HandleLoggingFailure(ex2);
        }
    }

    /// <summary>
    /// Writes a log entry to a file asynchronously, including the type of the log entry and the text to log.
    /// </summary>
    /// <param name="type">The type of the log entry.</param>
    /// <param name="text">The text to log.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [StswInfo("0.18.0")]
    private static async Task WriteInternal(StswInfoType? type, string text)
    {
        await _logSemaphore.WaitAsync();
        try
        {
            ForceSizeArchiveIfNeeded();

            var logPath = GetDailyLogFilePath();
            var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(type ?? StswInfoType.None).ToString()[0]} | {text}";

            Console.WriteLine(logLine);
            using var sw = new StreamWriter(logPath, true);
            await sw.WriteLineAsync(logLine);

            if (!Config.IsLoggingDisabled && Config.SqlLogger is not null)
                Config.SqlLogger.Invoke(new StswLogItem(type, text));

            CountLogToActiveCounters(type);
            _failureCount = 0;
        }
        catch (Exception ex)
        {
            HandleLoggingFailure(ex);
        }
        finally
        {
            _logSemaphore.Release();
        }
    }

    /// <summary>
    /// Writes a log entry to a file synchronously, including caller information such as member name, file path, and line number.
    /// </summary>
    /// <param name="type">Type of the log entry.</param>
    /// <param name="text">Text to log.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    [StswInfo("0.18.0")]
    public static void WriteWithCaller(StswInfoType? type, string text, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileName(filePath);
        text = $"[{fileName}:{lineNumber} {memberName}] {text}";
        Write(type, text);
    }

    /// <summary>
    /// Increments the active counters for the specified log type.
    /// </summary>
    /// <param name="type">The type of log entry to increment the counter for.</param>
    private static void CountLogToActiveCounters(StswInfoType? type)
    {
        if (type == null)
            return;

        lock (_logCounters)
        {
            foreach (var counter in _logCounters.Values)
            {
                if (counter.TryGetValue(type.Value, out var value))
                    counter[type.Value] = ++value;
                else
                    counter[type.Value] = 1;
            }
        }
    }

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
    /// Determines whether the log entry should be written based on the type of the log and the current configuration.
    /// </summary>
    /// <param name="type">The type of log entry.</param>
    /// <returns><see langword="true"/> if the log entry should be written; otherwise, <see langword="false"/>.</returns>
    private static bool ShouldLog(StswInfoType? type)
    {
        if (type == null)
            return true;

        return StswFn.IsInDebug
            ? type.Value.In(Config.LogTypes_DEBUG)
            : type.Value.In(Config.LogTypes_RELEASE);
    }
    #endregion

    /// <summary>
    /// Gets the path to the log file for the current day.
    /// </summary>
    /// <returns>The full path to the log file for today.</returns>
    private static string GetDailyLogFilePath() => Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log");
}
