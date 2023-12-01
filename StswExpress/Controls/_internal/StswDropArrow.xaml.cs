using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

internal class StswDropArrow : Control
{
    static StswDropArrow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDropArrow), new FrameworkPropertyMetadata(typeof(StswDropArrow)));
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswDropArrow)
        );
}
