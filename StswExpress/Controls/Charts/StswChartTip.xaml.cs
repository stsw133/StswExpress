using System.ComponentModel;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a tooltip for displaying info about chart element.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswChartTip : StswToolTip
{
    static StswChartTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartTip), new FrameworkPropertyMetadata(typeof(StswChartTip)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets whether the <see cref="StswChartElementModel.Description"/> property will be shown.
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
    /// Gets or sets whether the <see cref="StswChartElementModel.Name"/> property will be shown.
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
