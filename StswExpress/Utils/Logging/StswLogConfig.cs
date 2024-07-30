using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace StswExpress;
/// <summary>
/// Configuration settings for the <see cref="StswLog"/> class.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswLogConfig()
{
    /// <summary>
    /// Indicates whether to archive the full month's log files.
    /// </summary>
    public bool ArchiveFullMonth = true;

    /// <summary>
    /// Specifies the number of days to keep logs before archiving them.
    /// </summary>
    public int ArchiveUpToLastDays = 90;

    /// <summary>
    /// Specifies the number of days after which logs should be archived.
    /// </summary>
    public int ArchiveWhenDaysOver = 120;

    /*
    /// <summary>
    /// Specifies the file size threshold for archiving logs (in bytes).
    /// </summary>
    public int? ArchiveWhenSizeOver = 1024 * 1024 * 10;
    */

    /// <summary>
    /// Specifies the path to the directory where the archived log files will be saved.
    /// </summary>
    public string ArchiveDirectoryPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "archive");

    /// <summary>
    /// Specifies the path to the directory where the log files will be saved.
    /// </summary>
    public string LogDirectoryPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    /// <summary>
    /// Log types to include DEBUG mode. Any type that is not in this list will be skipped.
    /// </summary>
    public IEnumerable<StswInfoType> LogTypes_DEBUG { get; set; } = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>();

    /// <summary>
    /// Log types to include RELEASE mode. Any type that is not in this list will be skipped.
    /// </summary>
    public IEnumerable<StswInfoType> LogTypes_RELEASE { get; set; } = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>().Except([StswInfoType.Debug]);
}
