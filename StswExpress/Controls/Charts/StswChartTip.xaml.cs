using System.ComponentModel;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a tooltip used for displaying additional information about a chart element.
/// Supports showing the element's name and description based on configurable properties.
/// This control extends <see cref="StswToolTip"/> and is intended for internal use with chart components.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[StswInfo("0.4.0")]
public class StswChartTip : StswToolTip
{
    static StswChartTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartTip), new FrameworkPropertyMetadata(typeof(StswChartTip)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="StswChartElementModel.Description"/> property 
    /// should be displayed in the tooltip.
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
    /// Gets or sets a value indicating whether the <see cref="StswChartElementModel.Name"/> property 
    /// should be displayed in the tooltip.
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

/* usage:

<se:StswChartTip ShowName="True" ShowDescription="True"/>

*/
