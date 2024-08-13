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
    public bool ArchiveFullMonth { get; set; } = true;

    /// <summary>
    /// Specifies the number of days to keep logs before archiving them.
    /// </summary>
    public int ArchiveUpToLastDays { get; set; } = 90;

    /// <summary>
    /// Specifies the number of days after which logs should be archived.
    /// </summary>
    public int ArchiveWhenDaysOver { get; set; } = 120;

    /*
    /// <summary>
    /// Specifies the file size threshold for archiving logs (in bytes).
    /// </summary>
    public int? ArchiveWhenSizeOver { get; set; } = 1024 * 1024 * 10;
    */

    /// <summary>
    /// Specifies the path to the directory where the archived log files will be saved.
    /// </summary>
    public string ArchiveDirectoryPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "archive");

    /// <summary>
    /// Specifies the path to the directory where the log files will be saved.
    /// </summary>
    public string LogDirectoryPath
    {
        get
        {
            if (!Directory.Exists(_logDirectoryPath))
                Directory.CreateDirectory(_logDirectoryPath);
            return _logDirectoryPath;
        }
        set => _logDirectoryPath = value;
    }
    private string _logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    /// <summary>
    /// Log types to include DEBUG mode. Any type that is not in this list will be skipped.
    /// </summary>
    public IEnumerable<StswInfoType> LogTypes_DEBUG { get; set; } = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>();

    /// <summary>
    /// Log types to include RELEASE mode. Any type that is not in this list will be skipped.
    /// </summary>
    public IEnumerable<StswInfoType> LogTypes_RELEASE { get; set; } = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>().Except([StswInfoType.Debug]);

    /// <summary>
    /// Indicates whether logging is currently disabled.
    /// </summary>
    public bool IsLoggingDisabled { get; set; } = false;

    /// <summary>
    /// Specifies the maximum number of consecutive failures after which logging will be disabled.
    /// </summary>
    public int? MaxFailures { get; set; } = null;

    /// <summary>
    /// Defines a custom action to be executed when a logging error occurs.
    /// </summary>
    public Action<Exception>? OnLogFailure { get; set; }
}
