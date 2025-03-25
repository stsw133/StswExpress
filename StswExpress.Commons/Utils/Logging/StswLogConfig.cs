using System.ComponentModel;

namespace StswExpress.Commons;
/// <summary>
/// Configuration settings for the <see cref="StswLog"/> class, including options for archiving and log types.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswLogConfig()
{
    /// <summary>
    /// Specifies whether to automatically archive logs for a full month.
    /// </summary>
    public bool ArchiveFullMonth { get; set; } = true;

    /// <summary>
    /// Specifies the number of days to keep logs before considering them for archiving.
    /// </summary>
    public int ArchiveUpToLastDays { get; set; } = 90;

    /// <summary>
    /// Specifies the number of days after which logs should be archived.
    /// </summary>
    public int ArchiveWhenDaysOver { get; set; } = 120;

    /// <summary>
    /// Specifies the file size threshold for archiving logs (in bytes).
    /// </summary>
    public int? ArchiveWhenSizeOver { get; set; } = 1024 * 1024 * 5;
    
    /// <summary>
    /// Specifies the path to the directory where archived log files will be stored.
    /// </summary>
    public string ArchiveDirectoryPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "archive");

    /// <summary>
    /// Specifies the path to the directory where active log files will be stored.
    /// </summary>
    public string LogDirectoryPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    /// <summary>
    /// Specifies the log types to include in DEBUG mode. Any log type not in this list will be skipped.
    /// </summary>
    public IEnumerable<StswInfoType> LogTypes_DEBUG { get; set; } = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>();

    /// <summary>
    /// Specifies the log types to include in RELEASE mode. Any log type not in this list will be skipped.
    /// </summary>
    public IEnumerable<StswInfoType> LogTypes_RELEASE { get; set; } = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>().Except([StswInfoType.Debug]);

    /// <summary>
    /// Indicates whether logging is currently disabled.
    /// </summary>
    public bool IsLoggingDisabled { get; set; } = false;

    /// <summary>
    /// Specifies the maximum number of consecutive logging failures before logging is disabled.
    /// </summary>
    public int? MaxFailures { get; set; } = 3;

    /// <summary>
    /// Defines a custom action to execute when a logging error occurs.
    /// </summary>
    public Action<Exception>? OnLogFailure { get; set; }
}
