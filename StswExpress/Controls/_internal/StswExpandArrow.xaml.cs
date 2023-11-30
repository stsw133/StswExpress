using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

internal class StswExpandArrow : Control
{
    static StswExpandArrow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswExpandArrow), new FrameworkPropertyMetadata(typeof(StswExpandArrow)));
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
    public static readonly DependencyProperty IsExpandedProperty
        = DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(StswExpandArrow)
        );
}
