using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that extends the <see cref="ScrollViewer"/> class with additional functionality.
/// </summary>
public class StswScrollViewer : ScrollViewer
{
    static StswScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswScrollViewer), new FrameworkPropertyMetadata(typeof(StswScrollViewer)));
    }

    #region Events & methods
    /// <summary>
    /// Overrides the MouseWheel event to handle scrolling behavior.
    /// When the scroll reaches the top or bottom, it raises the MouseWheel event for the parent UIElement.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        /// horizontal scrolling
        if (ComputedHorizontalScrollBarVisibility == Visibility.Visible
        && (ComputedVerticalScrollBarVisibility != Visibility.Visible || Keyboard.Modifiers == ModifierKeys.Shift))
        {
            if (e.Delta > 0)
                ScrollInfo.MouseWheelLeft();
            else //if (e.Delta < 0)
                ScrollInfo.MouseWheelRight();

            e.Handled = true;
        }

        base.OnMouseWheel(e);

        /// scrolling scroll in another scroll
        //if (Parent is UIElement parentElement)
        //{
        //    if ((e.Delta > 0 && VerticalOffset == 0) || (e.Delta < 0 && VerticalOffset == ScrollableHeight))
        //    {
        //        e.Handled = true;
        //
        //        var routedArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        //        {
        //            RoutedEvent = MouseWheelEvent
        //        };
        //        parentElement.RaiseEvent(routedArgs);
        //    }
        //}
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets a value indicating whether the scroll bars are dynamic (automatically hide when are not used).
    /// </summary>
    public bool IsDynamic
    {
        get => (bool)GetValue(IsDynamicProperty);
        set => SetValue(IsDynamicProperty, value);
    }
    public static readonly DependencyProperty IsDynamicProperty
        = DependencyProperty.Register(
            nameof(IsDynamic),
            typeof(bool),
            typeof(StswScrollViewer)
        );
    #endregion
}

/// <summary>
/// Data model for properties of <see cref="StswScrollViewer"/> that is included in a control.
/// </summary>
public class StswScrollViewerModel : DependencyObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the content can be scrolled.
    /// </summary>
    public bool CanContentScroll
    {
        get => (bool)GetValue(CanContentScrollProperty);
        set => SetValue(CanContentScrollProperty, value);
    }
    public static readonly DependencyProperty CanContentScrollProperty
        = DependencyProperty.Register(
            nameof(CanContentScroll),
            typeof(bool),
            typeof(StswScrollViewerModel),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the scroll bars are dynamic (automatically hide when are not used).
    /// </summary>
    public bool IsDynamic
    {
        get => (bool)GetValue(IsDynamicProperty);
        set => SetValue(IsDynamicProperty, value);
    }
    public static readonly DependencyProperty IsDynamicProperty
        = DependencyProperty.Register(
            nameof(IsDynamic),
            typeof(bool),
            typeof(StswScrollViewerModel),
            new PropertyMetadata(false)
        );

    /// <summary>
    /// Gets or sets the panning mode, determining the allowed direction(s) for panning.
    /// </summary>
    public PanningMode PanningMode
    {
        get => (PanningMode)GetValue(PanningModeProperty);
        set => SetValue(PanningModeProperty, value);
    }
    public static readonly DependencyProperty PanningModeProperty
        = DependencyProperty.Register(
            nameof(PanningMode),
            typeof(PanningMode),
            typeof(StswScrollViewerModel),
            new PropertyMetadata(PanningMode.Both)
        );

    /// <summary>
    /// Gets or sets the visibility of the horizontal scroll bar.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }
    public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty
        = DependencyProperty.Register(
            nameof(HorizontalScrollBarVisibility),
            typeof(ScrollBarVisibility),
            typeof(StswScrollViewerModel),
            new PropertyMetadata(ScrollBarVisibility.Auto)
        );

    /// <summary>
    /// Gets or sets the visibility of the vertical scroll bar.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }
    public static readonly DependencyProperty VerticalScrollBarVisibilityProperty
        = DependencyProperty.Register(
            nameof(VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility),
            typeof(StswScrollViewerModel),
            new PropertyMetadata(ScrollBarVisibility.Auto)
        );
}
