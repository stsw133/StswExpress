using System.Collections.Generic;

namespace StswExpress.Wpf;

/// <summary>
/// Represents an entry in the navigation history.
/// </summary>
public class StswNavigationHistoryItem
{
    /// <summary>
    /// Gets or sets the navigation items representing the full path of the selection.
    /// </summary>
    public IReadOnlyList<StswNavigationItem> Path { get; init; } = [];

    /// <summary>
    /// Gets or sets the content associated with the navigation entry.
    /// </summary>
    public object? Content { get; init; }
}
