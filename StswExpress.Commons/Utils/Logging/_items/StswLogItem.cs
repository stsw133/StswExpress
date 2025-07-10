namespace StswExpress.Commons;
/// <summary>
/// Represents an immutable log entry with a type, text, and a timestamp.
/// </summary>
[StswInfo("0.9.0")]
public readonly struct StswLogItem(StswInfoType? type, string text, DateTime? dateTime = null)
{
    /// <summary>
    /// Gets the date and time of the log entry.
    /// </summary>
    public DateTime DateTime { get; } = dateTime ?? DateTime.Now;

    /// <summary>
    /// Gets the text of the log entry.
    /// </summary>
    public string? Text { get; } = text;

    /// <summary>
    /// Gets the type of the log entry.
    /// </summary>
    public StswInfoType? Type { get; } = type;
}
