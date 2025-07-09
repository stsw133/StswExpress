using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A <see cref="ScrollViewer"/> extension with dynamic scrolling behavior.
/// Supports automatic scrolling to the bottom and dynamic visibility of scrollbars.
/// </summary>
[Stsw("0.1.0")]
public class StswScrollView : ScrollViewer
{
    static StswScrollView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswScrollView), new FrameworkPropertyMetadata(typeof(StswScrollView)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        /// horizontal scrolling
        if ((ComputedHorizontalScrollBarVisibility == Visibility.Visible)
         && (ComputedVerticalScrollBarVisibility != Visibility.Visible || Keyboard.Modifiers == ModifierKeys.Shift))
        {
            if (e.Delta > 0)
                ScrollInfo.MouseWheelLeft();
            else
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

    /// <inheritdoc/>
    [Stsw("0.7.0")]
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
    /// If set to true, the content automatically scrolls to the bottom when new content is added.
    /// </summary>
    [Stsw("0.7.0")]
    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }
    public static readonly DependencyProperty AutoScrollProperty
        = DependencyProperty.RegisterAttached(
            nameof(AutoScroll),
            typeof(bool),
            typeof(StswScrollView),
            new PropertyMetadata(false)
        );
    public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);
    public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);

    /// <summary>
    /// Gets or sets the command associated with the control.
    /// This property allows binding a command to the scroll control that is executed under certain conditions.
    /// </summary>
    [Stsw("0.7.0")]
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.RegisterAttached(
            nameof(Command),
            typeof(ICommand),
            typeof(StswScrollView),
            new PropertyMetadata(default)
        );
    public static ICommand? GetCommand(DependencyObject obj) => (ICommand?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, ICommand? value) => obj.SetValue(CommandProperty, value);

    /// <summary>
    /// Gets or sets the parameter to pass to the command associated with the control.
    /// This allows passing additional data to the command when it is executed.
    /// </summary>
    [Stsw("0.7.0")]
    public object? CommandParameter
    {
        get => (object?)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.RegisterAttached(
            nameof(CommandParameter),
            typeof(object),
            typeof(StswScrollView),
            new PropertyMetadata(default)
        );
    public static object? GetCommandParameter(DependencyObject obj) => (object?)obj.GetValue(CommandParameterProperty);
    public static void SetCommandParameter(DependencyObject obj, object? value) => obj.SetValue(CommandParameterProperty, value);

    /// <summary>
    /// Gets or sets the target element on which to execute the command associated with the control.
    /// This allows specifying a target element to run the command on, different from the control itself.
    /// </summary>
    [Stsw("0.7.0")]
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }
    public static readonly DependencyProperty CommandTargetProperty
        = DependencyProperty.RegisterAttached(
            nameof(CommandTarget),
            typeof(IInputElement),
            typeof(StswScrollView),
            new PropertyMetadata(default)
        );
    public static IInputElement? GetCommandTarget(DependencyObject obj) => (IInputElement?)obj.GetValue(CommandTargetProperty);
    public static void SetCommandTarget(DependencyObject obj, IInputElement? value) => obj.SetValue(CommandTargetProperty, value);

    /// <summary>
    /// Gets or sets a value indicating whether the scroll bars are dynamic (automatically hide when not in use).
    /// If set to <see cref="StswScrollDynamicMode.Full"/>, the scrollbars are shown only when scrolling is needed, hiding when idle.
    /// </summary>
    [Stsw("0.19.0")]
    public StswScrollDynamicMode DynamicMode
    {
        get => (StswScrollDynamicMode)GetValue(DynamicModeProperty);
        set => SetValue(DynamicModeProperty, value);
    }
    public static readonly DependencyProperty DynamicModeProperty
        = DependencyProperty.RegisterAttached(
            nameof(DynamicMode),
            typeof(StswScrollDynamicMode),
            typeof(StswScrollView),
            new PropertyMetadata(StswScrollDynamicMode.Off)
        );
    public static StswScrollDynamicMode GetDynamicMode(DependencyObject obj) => (StswScrollDynamicMode)obj.GetValue(DynamicModeProperty);
    public static void SetDynamicMode(DependencyObject obj, StswScrollDynamicMode value) => obj.SetValue(DynamicModeProperty, value);

    /// <summary>
    /// Gets or sets a value indicating whether the scroll viewer is in a busy state.
    /// When set to true, the scroll viewer prevents user interactions, indicating a loading or processing state.
    /// </summary>
    [Stsw("0.7.0")]
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsBusy),
            typeof(bool?),
            typeof(StswScrollView),
            new PropertyMetadata(false)
        );
    public static bool GetIsBusy(DependencyObject obj) => (bool)obj.GetValue(IsBusyProperty);
    public static void SetIsBusy(DependencyObject obj, bool value) => obj.SetValue(IsBusyProperty, value);
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? BorderBrush { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Thickness? BorderThicknessProperty { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new HorizontalAlignment HorizontalContentAlignment { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new VerticalAlignment VerticalContentAlignment { get; private set; }
    #endregion
}

/* usage:

<se:StswScrollView AutoScroll="True">
    <StackPanel>
        <TextBlock Text="Scrollable Content"/>
        <TextBlock Text="More Content"/>
    </StackPanel>
</se:StswScrollView>

*/
