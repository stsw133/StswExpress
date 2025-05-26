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

    #region Archive
    /// <summary>
    /// Automatically archives log files based on the configuration settings.
    /// </summary>
    private static void AutoArchive()
    {
        var dir = new DirectoryInfo(Config.LogDirectoryPath);
        var oldestFileInfo = dir.GetFileSystemInfos("log_*.log").OrderBy(x => x.CreationTime).FirstOrDefault();
        if (oldestFileInfo == null)
            return;

        var oldestFileDT = oldestFileInfo.CreationTime;
        var dateNow = DateTime.Now.Date;

        if (Config.ArchiveFullMonth && !oldestFileDT.IsSameYearAndMonth(dateNow))
            foreach (var (year, month) in StswFn.GetUniqueMonthsFromRange(oldestFileDT, dateNow.AddMonths(-1)))
                Archive(new DateTime(year, month, 1), new DateTime(year, month, DateTime.DaysInMonth(year, month)));
        else if ((dateNow - oldestFileDT).TotalDays > Config.ArchiveWhenDaysOver)
                Archive(oldestFileDT, dateNow.AddDays(-Config.ArchiveUpToLastDays));

        DeleteOldArchives();
    }

    /// <summary>
    /// Archives log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    public static void Archive(DateTime dateFrom, DateTime dateTo)
    {
        string archiveName;
        if (Config.ArchiveFullMonth && dateFrom.IsSameYearAndMonth(dateTo))
            archiveName = $"archive_{dateFrom:yyyy-MM}.zip";
        else if (dateFrom == dateTo)
            archiveName = $"archive_{dateFrom:yyyy-MM-dd}.zip";
        else
            archiveName = $"archive_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.zip";

        var fullArchivePath = Path.Combine(Config.ArchiveDirectoryPath, archiveName);
        if (File.Exists(fullArchivePath))
            return;

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

    /// <summary>
    /// Archives log files for a single specified date.
    /// </summary>
    /// <param name="date">The date to archive.</param>
    public static void Archive(DateTime date) => Archive(date, date);

    /// <summary>
    /// Deletes log archives that are older than the specified number of days.
    /// </summary>
    private static void DeleteOldArchives()
    {
        if (!Config.DeleteArchivesOlderThanDays.HasValue || Config.DeleteArchivesOlderThanDays.Value <= 0)
            return;

        int limitDays = Config.DeleteArchivesOlderThanDays.Value;
        var thresholdDate = DateTime.Now.Date.AddDays(-limitDays);

        foreach (var zipPath in Directory.GetFiles(Config.ArchiveDirectoryPath, "archive_*.zip"))
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

    /// <summary>
    /// Adds a file to a zip archive, renaming it if a file with the same name already exists in the archive.
    /// </summary>
    /// <param name="zip"></param>
    /// <param name="sourceFilePath"></param>
    private static void AddFileToZipWithPossibleRename(ZipArchive zip, string sourceFilePath)
    {
        var baseFileName = Path.GetFileName(sourceFilePath);
        var datePart = baseFileName.Substring(4, 10);

        var sameDateEntries = zip.Entries.Where(e => e.FullName.StartsWith($"log_{datePart}", StringComparison.OrdinalIgnoreCase)).ToList();

        if (sameDateEntries.Count == 0)
        {
            zip.CreateEntryFromFile(sourceFilePath, baseFileName);
        }
        else if (sameDateEntries.Count == 1)
        {
            var existingEntry = sameDateEntries[0];
            var existingName = existingEntry.FullName;
            if (existingName.Equals($"log_{datePart}.log", StringComparison.OrdinalIgnoreCase))
                RenameZipArchiveEntry(zip, existingEntry, $"log_{datePart}_{DateTime.Now:HH-mm-ss}.log");

            var newName = $"log_{datePart}_{DateTime.Now:HH-mm-ss}.log";
            zip.CreateEntryFromFile(sourceFilePath, newName);
        }
        else
        {
            var newName = $"log_{datePart}_{DateTime.Now:HH-mm-ss}.log";
            zip.CreateEntryFromFile(sourceFilePath, newName);
        }
    }

    /// <summary>
    /// Archives a single log file based on its size.
    /// </summary>
    /// <param name="logFile"></param>
    private static void ArchiveSingleLogBySize(FileInfo logFile)
    {
        string datePart = logFile.Name.Substring(4, 10);
        string archiveFileName = $"archive_{datePart}.zip";

        var archivePath = Path.Combine(Config.ArchiveDirectoryPath, archiveFileName);
        using var zip = ZipFile.Open(archivePath, ZipArchiveMode.Update);

        AddFileToZipWithPossibleRename(zip, logFile.FullName);

        logFile.Delete();
    }

    /// <summary>
    /// Forces the archiving of the current log file if the size exceeds the threshold specified in <see cref="Config.ArchiveWhenSizeOver"/>.
    /// </summary>
    private static void ForceSizeArchiveIfNeeded()
    {
        if (Config.ArchiveWhenSizeOver == null)
            return;

        var path = GetDailyLogFilePath();
        var fi = new FileInfo(path);
        if (!fi.Exists)
            return;

        if (fi.Length > Config.ArchiveWhenSizeOver.Value)
            ArchiveSingleLogBySize(fi);
    }

    /// <summary>
    /// Renames a <see cref="ZipArchiveEntry"/> in a <see cref="ZipArchive"/> to a new name.
    /// </summary>
    /// <param name="zip"></param>
    /// <param name="entry"></param>
    /// <param name="newName"></param>
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
    /// <param name="fileName"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
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
    /// <param name="s"></param>
    /// <param name="yearMonth"></param>
    /// <returns></returns>
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

        for (DateTime d = dateFrom.Date; d <= dateTo.Date; d = d.AddDays(1))
        {
            var filesThisDate = Directory
                .GetFiles(Config.LogDirectoryPath, "log_*.log")
                .Where(p =>
                {
                    if (!TryGetDateFromFilename(p, out var fd)) return false;
                    return fd.Date == d.Date;
                })
                .ToList();

            foreach (var logFilePath in filesThisDate)
            {
                if (!File.Exists(logFilePath))
                    continue;

                var lines = await File.ReadAllLinesAsync(logFilePath);
                var currentBlock = new List<string>();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.Length >= 19 && DateTime.TryParse(line[..19], out var _))
                    {
                        if (currentBlock.Count > 0)
                        {
                            var parsed = ParseLogEntry(currentBlock);
                            if (parsed.HasValue)
                                logItems.Add(parsed.Value);
                            currentBlock.Clear();
                        }
                    }

                    currentBlock.Add(line);
                }

                if (currentBlock.Count > 0)
                {
                    var parsed = ParseLogEntry(currentBlock);
                    if (parsed.HasValue)
                        logItems.Add(parsed.Value);
                }
            }
        }

        return logItems;
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
        if (firstLine.Length < 19 || !DateTime.TryParse(firstLine[..19], out var date))
            return null;

        var typeChar = firstLine.Length > 22 ? firstLine[22] : (char?)null;
        var type = Enum.GetValues(typeof(StswInfoType))
                       .Cast<StswInfoType>()
                       .FirstOrDefault(t =>
                            t.ToString().StartsWith(typeChar?.ToString() ?? "",
                                                    StringComparison.OrdinalIgnoreCase));

        var textList = new List<string>
        {
            firstLine.Length > 26 ? firstLine[26..] : string.Empty
        };
        textList.AddRange(logEntryLines.Skip(1));

        var text = string.Join(Environment.NewLine, textList);
        return new StswLogItem(type, text, date);
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

        /// CREATE LOG
        try
        {
            ForceSizeArchiveIfNeeded();

            var logPath = GetDailyLogFilePath();
            var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(type ?? StswInfoType.None).ToString()[0]} | {text}";

            Console.WriteLine(logLine);
            using var sw = new StreamWriter(logPath, true);
            sw.WriteLine(logLine);
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

        /// CREATE LOG
        try
        {
            ForceSizeArchiveIfNeeded();

            var logPath = GetDailyLogFilePath();
            var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {type?.ToString().FirstOrDefault()} | {text}";

            Console.WriteLine(logLine);
            using var sw = new StreamWriter(logPath, true);
            await sw.WriteLineAsync(logLine);
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
    #endregion

    /// <summary>
    /// Gets the path to the log file for the current day.
    /// </summary>
    /// <returns></returns>
    private static string GetDailyLogFilePath() => Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log");
}
