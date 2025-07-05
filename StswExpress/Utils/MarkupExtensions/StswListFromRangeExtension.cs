using System;
using System.Linq;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A XAML markup extension that generates a list of integers based on the specified range.
/// The range can be provided in two formats:
/// - `"start-end"` (e.g., `"2-5"`) → Generates numbers from `start` to `end` (inclusive).
/// - `"count"` (e.g., `"3"`) → Generates numbers from `0` to `count - 1`.
/// <br/>
/// Example usages:
/// - `<ComboBox ItemsSource="{se:StswListFromRange 1-10}" />`
/// - `<ListBox ItemsSource="{se:StswListFromRange 5}" />`
/// </summary>
[Stsw("0.16.0", Changes = StswPlannedChanges.None)]
public class StswListFromRangeExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the range definition as a string (e.g., `"1-10"` or `"5"`).
    /// </summary>
    public string Definition { get; set; } = "0";

    /// <summary>
    /// Initializes a new instance of the <see cref="StswListFromRangeExtension"/> class with an optional range definition.
    /// </summary>
    /// <param name="definition">The range definition (e.g., `"1-10"` or `"5"`).</param>
    public StswListFromRangeExtension(string definition) => Definition = definition;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var parts = Definition.Split('-');

        if (parts.Length == 2 && int.TryParse(parts[0], out var start) && int.TryParse(parts[1], out var end))
            return Enumerable.Range(start, end - start + 1).ToList();

        if (parts.Length == 1 && int.TryParse(parts[0], out var count))
            return Enumerable.Range(0, count).ToList();

        return Array.Empty<int>();
    }
}

/* usage:

<ComboBox ItemsSource="{se:StswListFromRange 1-10}"/>

<ListBox ItemsSource="{se:StswListFromRange 5}"/>

*/
