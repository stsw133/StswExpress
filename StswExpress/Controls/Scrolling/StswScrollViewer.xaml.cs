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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        base.OnScrollChanged(e);
        
        if (GetAutoScroll(this) && !GetIsBusy(this))
        {
            if (e.ExtentHeightChange == 0)
                _autoScrolled = VerticalOffset == ScrollableHeight;
            if (_autoScrolled && e.ExtentHeightChange != 0)
                ScrollToVerticalOffset(ExtentHeight);
        }

        if (e.VerticalChange > 0)
            if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
                if (!GetIsBusy(this))
                    GetCommand(this)?.Execute(GetCommandParameter(this));
    }
    private bool _autoScrolled;
    #endregion

    #region Logic properties
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty AutoScrollProperty
        = DependencyProperty.RegisterAttached(
            "AutoScroll",
            typeof(bool),
            typeof(StswScrollViewer),
            new PropertyMetadata(false, OnAutoScrollChanged)
        );
    public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);
    public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);
    private static void OnAutoScrollChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                SetAutoScroll(scrollViewer, (bool)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content can be scrolled.
    /// </summary>
    public new static readonly DependencyProperty CanContentScrollProperty
        = DependencyProperty.RegisterAttached(
            nameof(CanContentScroll),
            typeof(bool),
            typeof(StswScrollViewer),
            new PropertyMetadata(false, OnCanContentScrollChanged)
        );
    public new static bool GetCanContentScroll(DependencyObject obj) => (bool)obj.GetValue(CanContentScrollProperty);
    public new static void SetCanContentScroll(DependencyObject obj, bool value) => obj.SetValue(CanContentScrollProperty, value);
    private static void OnCanContentScrollChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                SetCanContentScroll(scrollViewer, (bool)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the command associated with the control.
    /// </summary>
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.RegisterAttached(
            nameof(ICommandSource.Command),
            typeof(ICommand),
            typeof(StswScrollViewer),
            new PropertyMetadata(null, OnCommandChanged)
        );
    public static ICommand? GetCommand(DependencyObject obj) => (ICommand?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, ICommand? value) => obj.SetValue(CommandProperty, value);
    private static void OnCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                SetCommand(scrollViewer, (ICommand?)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command associated with the control.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.RegisterAttached(
            nameof(ICommandSource.CommandParameter),
            typeof(object),
            typeof(StswScrollViewer),
            new PropertyMetadata(null, OnCommandParameterChanged)
        );
    public static object? GetCommandParameter(DependencyObject obj) => (object?)obj.GetValue(CommandParameterProperty);
    public static void SetCommandParameter(DependencyObject obj, object? value) => obj.SetValue(CommandParameterProperty, value);
    private static void OnCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                SetCommandParameter(scrollViewer, (object?)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the target element on which to execute the command associated with the control.
    /// </summary>
    public static readonly DependencyProperty CommandTargetProperty
        = DependencyProperty.RegisterAttached(
            nameof(ICommandSource.CommandTarget),
            typeof(IInputElement),
            typeof(StswScrollViewer),
            new PropertyMetadata(null, OnCommandTargetChanged)
        );
    public static IInputElement? GetCommandTarget(DependencyObject obj) => (IInputElement?)obj.GetValue(CommandTargetProperty);
    public static void SetCommandTarget(DependencyObject obj, IInputElement? value) => obj.SetValue(CommandTargetProperty, value);
    private static void OnCommandTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                SetCommandTarget(scrollViewer, (IInputElement?)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the scroll viewer is in a busy state.
    /// </summary>
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswHeader.IsBusy),
            typeof(bool),
            typeof(StswScrollViewer),
            new PropertyMetadata(false, OnIsBusyChanged)
        );
    public static bool GetIsBusy(DependencyObject obj) => (bool)obj.GetValue(IsBusyProperty);
    public static void SetIsBusy(DependencyObject obj, bool value) => obj.SetValue(IsBusyProperty, value);
    private static void OnIsBusyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                SetIsBusy(scrollViewer, (bool)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the scroll bars are dynamic (automatically hide when are not used).
    /// </summary>
    public static readonly DependencyProperty IsDynamicProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswScrollBar.IsDynamic),
            typeof(bool),
            typeof(StswScrollViewer),
            new PropertyMetadata(false, OnIsDynamicChanged)
        );
    public static bool GetIsDynamic(DependencyObject obj) => (bool)obj.GetValue(IsDynamicProperty);
    public static void SetIsDynamic(DependencyObject obj, bool value) => obj.SetValue(IsDynamicProperty, value);
    private static void OnIsDynamicChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                SetIsDynamic(scrollViewer, (bool)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the panning mode, determining the allowed direction(s) for panning.
    /// </summary>
    public new static readonly DependencyProperty PanningModeProperty
        = DependencyProperty.RegisterAttached(
            nameof(PanningMode),
            typeof(PanningMode),
            typeof(StswScrollViewer),
            new PropertyMetadata(PanningMode.Both, OnPanningModeChanged)
        );
    public new static PanningMode GetPanningMode(DependencyObject obj) => (PanningMode)obj.GetValue(PanningModeProperty);
    public new static void SetPanningMode(DependencyObject obj, PanningMode value) => obj.SetValue(PanningModeProperty, value);
    private static void OnPanningModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                SetPanningMode(scrollViewer, (PanningMode)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the visibility of the horizontal scroll bar.
    /// </summary>
    public new static readonly DependencyProperty HorizontalScrollBarVisibilityProperty
        = DependencyProperty.RegisterAttached(
            nameof(HorizontalScrollBarVisibility),
            typeof(ScrollBarVisibility),
            typeof(StswScrollViewer),
            new PropertyMetadata(ScrollBarVisibility.Auto, OnHorizontalScrollBarVisibilityChanged)
        );
    public new static ScrollBarVisibility GetHorizontalScrollBarVisibility(DependencyObject obj) => (ScrollBarVisibility)obj.GetValue(HorizontalScrollBarVisibilityProperty);
    public new static void SetHorizontalScrollBarVisibility(DependencyObject obj, ScrollBarVisibility value) => obj.SetValue(HorizontalScrollBarVisibilityProperty, value);
    private static void OnHorizontalScrollBarVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.HorizontalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
        }
    }

    /// <summary>
    /// Gets or sets the visibility of the vertical scroll bar.
    /// </summary>
    public new static readonly DependencyProperty VerticalScrollBarVisibilityProperty
        = DependencyProperty.RegisterAttached(
            nameof(VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility),
            typeof(StswScrollViewer),
            new PropertyMetadata(ScrollBarVisibility.Auto, OnVerticalScrollBarVisibilityChanged)
        );
    public new static ScrollBarVisibility GetVerticalScrollBarVisibility(DependencyObject obj) => (ScrollBarVisibility)obj.GetValue(VerticalScrollBarVisibilityProperty);
    public new static void SetVerticalScrollBarVisibility(DependencyObject obj, ScrollBarVisibility value) => obj.SetValue(VerticalScrollBarVisibilityProperty, value);
    private static void OnVerticalScrollBarVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswScrollableControl stsw)
        {
            if (stsw.GetScrollViewer() is StswScrollViewer scrollViewer)
                scrollViewer.VerticalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
        }
    }
    #endregion
}
