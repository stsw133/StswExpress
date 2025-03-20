using System.ComponentModel;

namespace StswExpress.Commons;
/// <summary>
/// Provides configuration settings for managing email configurations in the application.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswMailboxesConfig
{
    /// <summary>
    /// Gets or sets the email address to use for all recipients when the application is in DEBUG mode.
    /// If this value is set, all outgoing emails will be sent to this address instead of the intended recipients.
    /// </summary>
    public string? DebugEmailRecipient { get; set; }

    /// <summary>
    /// Gets or sets the location of the file where encrypted email configurations are stored.
    /// </summary>
    public string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "mailboxes.stsw");

    /// <summary>
    /// Gets or sets a value indicating whether email sending is globally enabled. 
    /// If set to <see langword="false"/>, no emails will be sent by the application.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /*
    /// <summary>
    /// Gets or sets the maximum number of emails that can be sent by the application in a single day.
    /// </summary>
    public int? MaxEmailsPerDay { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of emails that can be sent by the application in a single hour.
    /// </summary>
    public int? MaxEmailsPerHour { get; set; }
    */
}
