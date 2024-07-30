using System;

namespace StswExpress;
/// <summary>
/// Represents a log item with a type and text.
/// </summary>
public struct StswLogItem(StswInfoType? type, string text)
{
    /// <summary>
    /// The date and time of the log entry.
    /// </summary>
    public DateTime DateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// The text of the log entry.
    /// </summary>
    public string? Text { get; set; } = text;

    /// <summary>
    /// The type of the log entry.
    /// </summary>
    public StswInfoType? Type { get; set; } = type;
}
