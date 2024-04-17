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
    /// Handles the ScrollChanged event to provide additional functionality on scroll change.
    /// </summary>
    /// <param name="e">The event arguments</param>
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
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether auto-scrolling is enabled.
    /// </summary>
    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }
    public static readonly DependencyProperty AutoScrollProperty
        = DependencyProperty.RegisterAttached(
            nameof(AutoScroll),
            typeof(bool),
            typeof(StswScrollViewer),
            new PropertyMetadata(false)
        );
    public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);
    public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);
    
    /// <summary>
    /// Gets or sets the command associated with the control.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.RegisterAttached(
            nameof(ICommandSource.Command),
            typeof(ICommand),
            typeof(StswScrollViewer),
            new PropertyMetadata(default)
        );
    public static ICommand? GetCommand(DependencyObject obj) => (ICommand?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, ICommand? value) => obj.SetValue(CommandProperty, value);

    /// <summary>
    /// Gets or sets the parameter to pass to the command associated with the control.
    /// </summary>
    public object? CommandParameter
    {
        get => (object?)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.RegisterAttached(
            nameof(ICommandSource.CommandParameter),
            typeof(object),
            typeof(StswScrollViewer),
            new PropertyMetadata(default)
        );
    public static object? GetCommandParameter(DependencyObject obj) => (object?)obj.GetValue(CommandParameterProperty);
    public static void SetCommandParameter(DependencyObject obj, object? value) => obj.SetValue(CommandParameterProperty, value);

    /// <summary>
    /// Gets or sets the target element on which to execute the command associated with the control.
    /// </summary>
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }
    public static readonly DependencyProperty CommandTargetProperty
        = DependencyProperty.RegisterAttached(
            nameof(ICommandSource.CommandTarget),
            typeof(IInputElement),
            typeof(StswScrollViewer),
            new PropertyMetadata(default)
        );
    public static IInputElement? GetCommandTarget(DependencyObject obj) => (IInputElement?)obj.GetValue(CommandTargetProperty);
    public static void SetCommandTarget(DependencyObject obj, IInputElement? value) => obj.SetValue(CommandTargetProperty, value);

    /// <summary>
    /// Gets or sets a value indicating whether the scroll viewer is in a busy state.
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.RegisterAttached(
            nameof(StswHeader.IsBusy),
            typeof(bool),
            typeof(StswScrollViewer),
            new PropertyMetadata(false)
        );
    public static bool GetIsBusy(DependencyObject obj) => (bool)obj.GetValue(IsBusyProperty);
    public static void SetIsBusy(DependencyObject obj, bool value) => obj.SetValue(IsBusyProperty, value);

    /// <summary>
    /// Gets or sets a value indicating whether the scroll bars are dynamic (automatically hide when are not used).
    /// </summary>
    public bool IsDynamic
    {
        get => (bool)GetValue(IsDynamicProperty);
        set => SetValue(IsDynamicProperty, value);
    }
    public static readonly DependencyProperty IsDynamicProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsDynamic),
            typeof(bool),
            typeof(StswScrollViewer),
            new PropertyMetadata(false)
        );
    public static bool GetIsDynamic(DependencyObject obj) => (bool)obj.GetValue(IsDynamicProperty);
    public static void SetIsDynamic(DependencyObject obj, bool value) => obj.SetValue(IsDynamicProperty, value);
    #endregion
}
