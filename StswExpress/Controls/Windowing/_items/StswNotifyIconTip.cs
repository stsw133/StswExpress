using System.Windows.Forms;

namespace StswExpress;

/// <summary>
/// Data model representing the details of a notification tip in the <see cref="StswNotifyIcon"/> control, including title, text, and icon type.
/// </summary>
[Stsw("0.1.0")]
public struct StswNotifyIconTip(string title, string text, ToolTipIcon icon)
{
    /// <summary>
    /// Gets or sets the title of the notification tip.
    /// </summary>
    public string TipTitle { get; set; } = title;

    /// <summary>
    /// Gets or sets the text of the notification tip.
    /// </summary>
    public string TipText { get; set; } = text;

    /// <summary>
    /// Gets or sets the icon type of the notification tip.
    /// </summary>
    public ToolTipIcon TipIcon { get; set; } = icon;
}
