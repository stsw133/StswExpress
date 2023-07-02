using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

public class StswScrollViewer : ScrollViewer
{
    static StswScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswScrollViewer), new FrameworkPropertyMetadata(typeof(StswScrollViewer)));
    }

    #region Events
    /// OnMouseWheel
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (Parent is UIElement parentElement)
        {
            if ((e.Delta > 0 && VerticalOffset == 0) ||
                (e.Delta < 0 && VerticalOffset == ScrollableHeight))
            {
                e.Handled = true;

                var routedArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                routedArgs.RoutedEvent = MouseWheelEvent;
                parentElement.RaiseEvent(routedArgs);
            }
        }

        base.OnMouseWheel(e);
    }
    #endregion

    #region Spatial properties
    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswScrollViewer)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
