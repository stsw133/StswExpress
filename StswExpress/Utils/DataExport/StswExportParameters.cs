namespace StswExpress;
/// <summary>
/// A struct for specifying additional export parameters, such as the default name of the exported file.
/// </summary>
public struct StswExportParameters
{
    public StswExportParameters()
    {
    }

    /// <summary>
    /// Gets or sets the default extension for the file dialog.
    /// </summary>
    public string? FileDialogDefaultExt { get; set; } = null;

    /// <summary>
    /// Gets or sets the filter for the file dialog.
    /// </summary>
    public string? FileDialogFilter { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether to include properties that do not have the <see cref="StswExportAttribute"/> attribute.
    /// </summary>
    public bool IncludeNonAttributed { get; set; } = false;

    /// <summary>
    /// Gets or sets the recommended file name for the exported file.
    /// </summary>
    public string RecommendedFileName { get; set; } = string.Empty;
}
