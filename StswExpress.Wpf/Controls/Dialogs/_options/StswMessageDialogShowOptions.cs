namespace StswExpress.Wpf;

/// <summary>
/// Provides optional parameters for displaying a <see cref="StswMessageDialog"/>.
/// </summary>
public readonly struct StswMessageDialogShowOptions
{
    /// <summary>
    /// Gets the email address used for the "Send email" action.
    /// </summary>
    public string? MailAddress { get; init; }

    /// <summary>
    /// Gets a value indicating whether the dialog content should be logged.
    /// </summary>
    public bool SaveLog { get; init; }
}
