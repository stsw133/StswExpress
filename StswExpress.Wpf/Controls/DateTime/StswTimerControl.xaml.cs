using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace StswExpress.Wpf;
/// <summary>
/// Represents a customizable timer control that can count up or down.
/// Supports different time formats, adjustable start and end times, and automatic updates using a <see cref="DispatcherTimer"/>.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTimerControl StartTime="01:00:00" EndTime="00:00:00" Format="hh\:mm\:ss" IsCountingDown="True" IsRunning="True"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(EndTime))]
public class StswTimerControl : Control
{
    private readonly DispatcherTimer _timer = new();
    private TextBlock? _display;
    private bool _isTimerTickSubscribed;

    public StswTimerControl()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        UpdateTimerInterval();
        SubscribeToTimerTick();
    }
    static StswTimerControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimerControl), new FrameworkPropertyMetadata(typeof(StswTimerControl)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _display = GetTemplateChild("PART_Display") as TextBlock;

        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Handles the Loaded event of the control, subscribing to the timer tick event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e) => SubscribeToTimerTick();

    /// <summary>
    /// Handles the Unloaded event of the control, stopping the timer and unsubscribing from the timer tick event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        UnsubscribeFromTimerTick();
    }

    /// <summary>
    /// Subscribes to the timer's tick event to update the current time.
    /// </summary>
    private void SubscribeToTimerTick()
    {
        if (_isTimerTickSubscribed)
            return;

        _timer.Tick += Timer_Tick;
        _isTimerTickSubscribed = true;
    }

    /// <summary>
    /// Unsubscribes from the timer's tick event to stop updating the current time.
    /// </summary>
    private void UnsubscribeFromTimerTick()
    {
        if (!_isTimerTickSubscribed)
            return;

        _timer.Tick -= Timer_Tick;
        _isTimerTickSubscribed = false;
    }

    /// <summary>
    /// Handles the tick event of the timer, updating the <see cref="CurrentTime"/> in fixed steps based on the timer <see cref="_timer.Interval"/>.
    /// Stops the timer when the end time is reached.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!IsRunning)
            return;

        var step = _timer.Interval;
        if (step <= TimeSpan.Zero)
            step = TimeSpan.FromSeconds(1);

        var finished = false;

        if (IsCountingDown)
        {
            var next = CurrentTime - step;
            if (next <= EndTime || next <= TimeSpan.Zero)
            {
                finished = true;
                CurrentTime = EndTime;
            }
            else
            {
                CurrentTime = next;
            }
        }
        else
        {
            var next = CurrentTime + step;
            if (next >= EndTime)
            {
                finished = true;
                CurrentTime = EndTime;
            }
            else
            {
                CurrentTime = next;
            }
        }

        if (!finished)
            return;

        IsRunning = false;
        _timer.Stop();

        switch (Command)
        {
            case RoutedCommand routedCommand when routedCommand.CanExecute(CommandParameter, CommandTarget ?? this):
                routedCommand.Execute(CommandParameter, CommandTarget ?? this);
                break;
            case ICommand command when command.CanExecute(CommandParameter):
                command.Execute(CommandParameter);
                break;
        }
    }

    /// <summary>
    /// Adjusts the timer's interval based on the <see cref="Format"/>.
    /// The interval is dynamically set to match the required precision, such as milliseconds, seconds, or minutes.
    /// </summary>
    private void UpdateTimerInterval() => _timer.Interval = Format switch
    {
        null => TimeSpan.FromSeconds(1),
        var fmt when fmt.Contains("fff") => TimeSpan.FromMilliseconds(1),
        var fmt when fmt.Contains("ff") => TimeSpan.FromMilliseconds(10),
        var fmt when fmt.Contains('f') => TimeSpan.FromMilliseconds(100),
        var fmt when fmt.Contains("ss") => TimeSpan.FromSeconds(1),
        var fmt when fmt.Contains("mm") => TimeSpan.FromMinutes(1),
        var fmt when fmt.Contains("hh") => TimeSpan.FromHours(1),
        _ => TimeSpan.FromSeconds(1),
    };
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the command executed when the timer reaches <see cref="EndTime"/>.
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
            typeof(StswTimerControl)
        );

    /// <summary>
    /// Gets or sets the parameter passed to <see cref="EndCommand"/> when executed.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(StswTimerControl)
        );

    /// <summary>
    /// Gets or sets the target element for <see cref="EndCommand"/> when it is a <see cref="RoutedCommand"/>.
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
            typeof(StswTimerControl)
        );

    /// <summary>
    /// Gets or sets the current time displayed by the timer.
    /// This value updates dynamically as the timer progresses.
    /// </summary>
    public TimeSpan CurrentTime
    {
        get => (TimeSpan)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }
    public static readonly DependencyProperty CurrentTimeProperty
        = DependencyProperty.Register(
            nameof(CurrentTime),
            typeof(TimeSpan),
            typeof(StswTimerControl)
        );

    /// <summary>
    /// Gets or sets the end time for the timer.
    /// Defines the target time when counting down or the maximum time when counting up.
    /// </summary>
    public TimeSpan EndTime
    {
        get => (TimeSpan)GetValue(EndTimeProperty);
        set => SetValue(EndTimeProperty, value);
    }
    public static readonly DependencyProperty EndTimeProperty
        = DependencyProperty.Register(
            nameof(EndTime),
            typeof(TimeSpan),
            typeof(StswTimerControl)
        );

    /// <summary>
    /// Gets or sets the format string used to display the current time.
    /// Controls the precision and appearance of the displayed time.
    /// </summary>
    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswTimerControl),
            new FrameworkPropertyMetadata(default(string), OnFormatChanged)
        );
    public static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswTimerControl stsw)
            return;

        stsw._display?.SetBinding(TextBlock.TextProperty, new Binding(nameof(CurrentTime))
        {
            Source = stsw,
            StringFormat = stsw.Format
        });
        stsw.UpdateTimerInterval();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer counts down from the start time to the end time,
    /// or counts up from the start time towards the end time.
    /// </summary>
    public bool IsCountingDown
    {
        get => (bool)GetValue(IsCountingDownProperty);
        set => SetValue(IsCountingDownProperty, value);
    }
    public static readonly DependencyProperty IsCountingDownProperty
        = DependencyProperty.Register(
            nameof(IsCountingDown),
            typeof(bool),
            typeof(StswTimerControl)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the timer is currently running.
    /// When set to <see langword="true"/>, the timer starts counting. When set to <see langword="false"/>, it stops.
    /// If <see cref="ResetOnStart"/> is <see langword="true"/>, starting the timer resets <see cref="CurrentTime"/> to <see cref="StartTime"/>.
    /// </summary>
    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }
    public static readonly DependencyProperty IsRunningProperty
        = DependencyProperty.Register(
            nameof(IsRunning),
            typeof(bool),
            typeof(StswTimerControl),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsRunningChanged)
        );
    public static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswTimerControl stsw)
            return;

        var isRunning = (bool)e.NewValue;

        if (isRunning)
        {
            if (stsw.ResetOnStart)
                stsw.CurrentTime = stsw.StartTime;

            stsw._timer.Start();
        }
        else
        {
            stsw._timer.Stop();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer should reset <see cref="CurrentTime"/> to <see cref="StartTime"/> when <see cref="IsRunning"/> is set to <see langword="true"/>.
    /// </summary>
    public bool ResetOnStart
    {
        get => (bool)GetValue(ResetOnStartProperty);
        set => SetValue(ResetOnStartProperty, value);
    }
    public static readonly DependencyProperty ResetOnStartProperty
        = DependencyProperty.Register(
            nameof(ResetOnStart),
            typeof(bool),
            typeof(StswTimerControl)
        );

    /// <summary>
    /// Gets or sets the start time for the timer.
    /// This is the initial time from which the timer begins counting.
    /// </summary>
    public TimeSpan StartTime
    {
        get => (TimeSpan)GetValue(StartTimeProperty);
        set => SetValue(StartTimeProperty, value);
    }
    public static readonly DependencyProperty StartTimeProperty
        = DependencyProperty.Register(
            nameof(StartTime),
            typeof(TimeSpan),
            typeof(StswTimerControl)
        );
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswTimerControl)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswTimerControl)
        );
    #endregion
}
