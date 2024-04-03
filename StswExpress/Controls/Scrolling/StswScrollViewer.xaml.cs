using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a control that extends the <see cref="ScrollViewer"/> class with additional functionality.
/// </summary>
public class StswScrollViewer : ScrollViewer, ICommandSource
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
        
        if (AutoScroll && !IsBusy)
        {
            if (e.ExtentHeightChange == 0)
                _autoScrolled = VerticalOffset == ScrollableHeight;
            if (_autoScrolled && e.ExtentHeightChange != 0)
                ScrollToVerticalOffset(ExtentHeight);
        }

        if (e.VerticalChange > 0)
            if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
                if (!IsBusy)
                    Command?.Execute(CommandParameter);
    }
    private bool _autoScrolled;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    public void InitAttachedProperties(DependencyObject obj)
    {
        AutoScroll = StswScrollControl.GetAutoScroll(obj);
        CanContentScroll = StswScrollControl.GetCanContentScroll(obj);
        Command = StswScrollControl.GetCommand(obj);
        CommandParameter = StswScrollControl.GetCommandParameter(obj);
        CommandTarget = StswScrollControl.GetCommandTarget(obj);
        IsBusy = StswScrollControl.GetIsBusy(obj);
        IsDynamic = StswScrollControl.GetIsDynamic(obj);
        PanningMode = StswScrollControl.GetPanningMode(obj);
        HorizontalScrollBarVisibility = StswScrollControl.GetHorizontalScrollBarVisibility(obj);
        VerticalScrollBarVisibility = StswScrollControl.GetVerticalScrollBarVisibility(obj);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// 
    /// </summary>
    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }
    public static readonly DependencyProperty AutoScrollProperty
        = DependencyProperty.Register(
            nameof(AutoScroll),
            typeof(bool),
            typeof(StswScrollViewer)
        );

    /// <summary>
    /// Gets or sets the command associated with the control.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(StswScrollViewer)
        );

    /// <summary>
    /// Gets or sets the parameter to pass to the command associated with the control.
    /// </summary>
    public object? CommandParameter
    {
        get => (object?)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(StswScrollViewer)
        );

    /// <summary>
    /// Gets or sets the target element on which to execute the command associated with the control.
    /// </summary>
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }
    public static readonly DependencyProperty CommandTargetProperty
        = DependencyProperty.Register(
            nameof(CommandTarget),
            typeof(IInputElement),
            typeof(StswScrollViewer)
        );

    /// <summary>
    /// 
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswScrollViewer)
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
            typeof(StswScrollViewer)
        );
    #endregion
}
