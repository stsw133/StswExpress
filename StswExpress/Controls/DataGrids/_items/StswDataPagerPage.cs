namespace StswExpress;

/// <summary>
/// Data model for <see cref="StswDataPager"/>'s page buttons.
/// </summary>
[StswInfo("0.5.0")]
internal struct StswDataPagerPage(string description, int page, bool isEnabled)
{
    /// <summary>
    /// Gets or sets the content displayed on the button.
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// Gets or sets the page number to which the button navigates when clicked.
    /// </summary>
    public int Page { get; set; } = page;

    /// <summary>
    /// Gets or sets a value indicating whether the button is enabled for interaction.
    /// </summary>
    public bool IsEnabled { get; set; } = isEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether the button is selected.
    /// </summary>
    public bool IsSelected { get; set; }
}
