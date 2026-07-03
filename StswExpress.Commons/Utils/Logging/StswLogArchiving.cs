using System.Globalization;
using System.IO.Compression;

namespace StswExpress.Commons;

/// <summary>
/// Provides log archiving operations for <see cref="StswLog"/>.
/// </summary>
public static class StswLogArchiving
{
    /// <summary>
    /// Log file search pattern.
    /// </summary>
    internal const string LogFilePattern = "log_*.log";

	/// <summary>
	/// Tries to extract a date from a log file name.
	/// </summary>
	/// <param name="filePath">The path of the log file.</param>
	/// <param name="date">The extracted date if successful.</param>
	/// <returns><see langword="true"/> if the date was successfully extracted; otherwise, <see langword="false"/>.</returns>
	internal static bool TryGetLogDate(string filePath, out DateTime date)
    {
        date = default;
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        if (!fileName.StartsWith("log_"))
            return false;

        if (fileName.Length < 14)
            return false;

        var datePart = fileName.Substring(4, 10);
        return DateTime.TryParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    /// <summary>
    /// Ensures required archive paths exist and runs automatic archiving.
    /// </summary>
    internal static void Initialize()
    {
		if (StswLog.Config.Archive.IsArchivingDisabled)
			return;

		EnsureArchiveDirectoryExists();
		AutoArchive();
	}

    /// <summary>
    /// Ensures the archive directory exists.
    /// </summary>
    private static void EnsureArchiveDirectoryExists()
    {
        if (!string.IsNullOrEmpty(StswLog.Config.Archive.ArchiveDirectoryPath) && !Directory.Exists(StswLog.Config.Archive.ArchiveDirectoryPath))
            Directory.CreateDirectory(StswLog.Config.Archive.ArchiveDirectoryPath);
    }

    /// <summary>
    /// Automatically archives log files based on the configuration settings.
    /// </summary>
    public static void AutoArchive()
    {
		if (StswLog.Config.Archive.IsArchivingDisabled)
			return;
		
        StswLog.EnsureLogDirectoryExists();
        EnsureArchiveDirectoryExists();

        var dir = new DirectoryInfo(StswLog.Config.LogDirectoryPath);
        var oldestFileInfo = dir.GetFileSystemInfos(LogFilePattern).OrderBy(x => x.CreationTime).FirstOrDefault();
        if (oldestFileInfo == null)
            return;

        var oldestFileDT = oldestFileInfo.CreationTime;
        var dateNow = DateTime.Now.Date;

        if (StswLog.Config.Archive.ArchiveFullMonth && !oldestFileDT.IsSameYearAndMonth(dateNow))
            foreach (var month in new StswDateRange(oldestFileDT, dateNow.AddMonths(-1)).GetUniqueMonthDates())
                Archive(month, month.ToLastDayOfMonth());
        else if ((dateNow - oldestFileDT).TotalDays > StswLog.Config.Archive.ArchiveWhenDaysOver)
            Archive(oldestFileDT, dateNow.AddDays(-StswLog.Config.Archive.ArchiveUpToLastDays));

        DeleteOldArchives();
    }

    /// <summary>
    /// Archives log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    public static void Archive(DateTime dateFrom, DateTime dateTo)
	{
		if (StswLog.Config.Archive.IsArchivingDisabled)
			return;

		StswLog.FileSemaphore.Wait();
        try
        {
            StswLog.EnsureLogDirectoryExists();
            EnsureArchiveDirectoryExists();

            string archiveName;
            if (StswLog.Config.Archive.ArchiveFullMonth && dateFrom.IsSameYearAndMonth(dateTo))
                archiveName = $"archive_{dateFrom:yyyy-MM}.zip";
            else if (dateFrom == dateTo)
                archiveName = $"archive_{dateFrom:yyyy-MM-dd}.zip";
            else
                archiveName = $"archive_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.zip";

            var fullArchivePath = Path.Combine(StswLog.Config.Archive.ArchiveDirectoryPath, archiveName);

            using var archive = ZipFile.Open(fullArchivePath, ZipArchiveMode.Update);
            foreach (var filePath in Directory.GetFiles(StswLog.Config.LogDirectoryPath, LogFilePattern))
            {
                if (!TryGetLogDate(filePath, out var fileDate))
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
            StswLog.FileSemaphore.Release();
        }
    }

    /// <summary>
    /// Archives log files for a single specified date.
    /// </summary>
    /// <param name="date">The date to archive.</param>
    public static void Archive(DateTime date) => Archive(date, date);

    /// <summary>
    /// Forces the archiving of the current log file if the size exceeds the configured threshold.
    /// </summary>
    internal static void ForceSizeArchiveIfNeeded()
    {
		if (StswLog.Config.Archive.IsArchivingDisabled)
			return;
		
        if (StswLog.Config.Archive.ArchiveWhenSizeOver == null)
            return;

        EnsureArchiveDirectoryExists();

        var path = StswLog.GetDailyLogFilePath();
        var fi = new FileInfo(path);
        if (!fi.Exists)
            return;

        if (fi.Length > StswLog.Config.Archive.ArchiveWhenSizeOver.Value)
            ArchiveSingleLogBySize(fi);
    }

    /// <summary>
    /// Deletes log archives that are older than the configured number of days.
    /// </summary>
    private static void DeleteOldArchives()
	{
		if (StswLog.Config.Archive.IsArchivingDisabled)
			return;
		
        StswLog.FileSemaphore.Wait();
        try
        {
            StswLog.EnsureLogDirectoryExists();
            EnsureArchiveDirectoryExists();

            if (!StswLog.Config.Archive.DeleteArchivesOlderThanDays.HasValue || StswLog.Config.Archive.DeleteArchivesOlderThanDays.Value <= 0)
                return;

            int limitDays = StswLog.Config.Archive.DeleteArchivesOlderThanDays.Value;
            var thresholdDate = DateTime.Now.Date.AddDays(-limitDays);

            foreach (var zipPath in Directory.GetFiles(StswLog.Config.Archive.ArchiveDirectoryPath, "archive_*.zip"))
            {
                var fileName = Path.GetFileNameWithoutExtension(zipPath);

                if (TryGetArchiveDateRange(fileName, out _, out var dateTo) && dateTo.Date < thresholdDate)
                {
                    try
                    {
                        File.Delete(zipPath);
                    }
                    catch (Exception ex)
                    {
                        StswLog.Config.OnLogFailure?.Invoke(ex);
                    }
                }
            }
        }
        finally
        {
            StswLog.FileSemaphore.Release();
        }
    }

    /// <summary>
    /// Archives a single log file based on its size.
    /// </summary>
    /// <param name="logFile">The log file to archive.</param>
    private static void ArchiveSingleLogBySize(FileInfo logFile)
    {
        string datePart = logFile.Name.Substring(4, 10);
        string archiveFileName = $"archive_{datePart}.zip";

        var archivePath = Path.Combine(StswLog.Config.Archive.ArchiveDirectoryPath, archiveFileName);
        using var zip = ZipFile.Open(archivePath, ZipArchiveMode.Update);

        AddFileToZipWithPossibleRename(zip, logFile.FullName);

        logFile.Delete();
    }

    /// <summary>
    /// Adds a file to a zip archive, renaming it if a file with the same name already exists in the archive.
    /// </summary>
    /// <param name="zip">The zip archive to add the file to.</param>
    /// <param name="sourceFilePath">The path of the file to add to the archive.</param>
    private static void AddFileToZipWithPossibleRename(ZipArchive zip, string sourceFilePath)
    {
        var baseFileName = Path.GetFileName(sourceFilePath);
        var datePart = baseFileName.Substring(4, 10);
        var extension = Path.GetExtension(baseFileName);
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
    /// Renames a <see cref="ZipArchiveEntry"/> in a <see cref="ZipArchive"/> to a new name.
    /// </summary>
    /// <param name="zip">The zip archive containing the entry.</param>
    /// <param name="entry">The entry to rename.</param>
    /// <param name="newName">The new name for the entry.</param>
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
    /// <param name="archiveName">The name of the archive file without path or extension.</param>
    /// <param name="dateFrom">The extracted start date if successful.</param>
    /// <param name="dateTo">The extracted end date if successful.</param>
    /// <returns><see langword="true"/> if the date range was successfully extracted; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetArchiveDateRange(string archiveName, out DateTime dateFrom, out DateTime dateTo)
    {
        dateFrom = default;
        dateTo = default;

        if (!archiveName.StartsWith("archive_"))
            return false;

        var rest = archiveName.Substring("archive_".Length);
        var parts = rest.Split(['_'], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            if (TryParseYearMonth(parts[0], out var ymFrom))
            {
                dateFrom = new DateTime(ymFrom.Year, ymFrom.Month, 1);
                dateTo = dateFrom.AddMonths(1).AddDays(-1);
                return true;
            }

            if (DateTime.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var singleDate))
            {
                dateFrom = singleDate.Date;
                dateTo = singleDate.Date;
                return true;
            }

            return false;
        }

        if (parts.Length == 2)
        {
            if (TryParseYearMonth(parts[0], out var ym1) && TryParseYearMonth(parts[1], out var ym2))
            {
                dateFrom = new DateTime(ym1.Year, ym1.Month, 1);
                dateTo = new DateTime(ym2.Year, ym2.Month, 1).AddMonths(1).AddDays(-1);
                return true;
            }

            var success1 = DateTime.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dd1);
            var success2 = DateTime.TryParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dd2);

            if (success1 && success2)
            {
                dateFrom = dd1.Date < dd2.Date ? dd1.Date : dd2.Date;
                dateTo = dd1.Date > dd2.Date ? dd1.Date : dd2.Date;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to parse a string in the format yyyy-MM into a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="text">The string to parse.</param>
    /// <param name="yearMonth">The parsed <see cref="DateTime"/> representing the first day of the month.</param>
    /// <returns><see langword="true"/> if the parsing was successful; otherwise, <see langword="false"/>.</returns>
    private static bool TryParseYearMonth(string text, out DateTime yearMonth)
    {
        yearMonth = default;
        if (DateTime.TryParseExact(text, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            yearMonth = new DateTime(dt.Year, dt.Month, 1);
            return true;
        }

        return false;
    }
}
