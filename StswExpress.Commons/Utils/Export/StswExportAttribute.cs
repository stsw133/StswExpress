namespace StswExpress.Commons;
/// <summary>
/// Attribute used to control the exporting behavior for specific properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StswExportAttribute(string? columnName = null, string? columnFormat = null, bool isColumnIgnored = false, int order = 0) : Attribute
{
    /// <summary>
    /// Gets or sets the custom name to be used as the column header in the exported file.
    /// </summary>
    public string? ColumnName { get; set; } = columnName;

    /// <summary>
    /// Gets or sets the custom format string to be used for formatting the property's value in the exported file.
    /// </summary>
    public string? ColumnFormat { get; set; } = columnFormat;

    /// <summary>
    /// Gets or sets a flag indicating whether to ignore exporting the property as a column in the file.
    /// </summary>
    public bool IsColumnIgnored { get; set; } = isColumnIgnored;

    /// <summary>
    /// Gets or sets the order in which the column should appear in the exported file.
    /// </summary>
    public int Order { get; set; } = order;
}
