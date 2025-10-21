using System.ComponentModel;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a tooltip used for displaying additional information about a chart element.
/// Supports showing the element's name and description based on configurable properties.
/// This control extends <see cref="StswToolTip"/> and is intended for internal use with chart components.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswChartTip ShowName="True" ShowDescription="True"/&gt;
/// </code>
/// </example>
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswChartTip : StswToolTip
{
    static StswChartTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartTip), new FrameworkPropertyMetadata(typeof(StswChartTip)));
    }

    #region Logic properties
    /// <summary>
    /// Indicates whether the description of the chart element should be displayed in the tooltip.
    /// </summary>
    public bool ShowDescription
    {
        get => (bool)GetValue(ShowDescriptionProperty);
        set => SetValue(ShowDescriptionProperty, value);
    }
    public static readonly DependencyProperty ShowDescriptionProperty
        = DependencyProperty.Register(
            nameof(ShowDescription),
            typeof(bool),
            typeof(StswChartTip)
        );

    /// <summary>
    /// Indicates whether the name of the chart element should be displayed in the tooltip.
    /// </summary>
    public bool ShowName
    {
        get => (bool)GetValue(ShowNameProperty);
        set => SetValue(ShowNameProperty, value);
    }
    public static readonly DependencyProperty ShowNameProperty
        = DependencyProperty.Register(
            nameof(ShowName),
            typeof(bool),
            typeof(StswChartTip)
        );
    #endregion
}
