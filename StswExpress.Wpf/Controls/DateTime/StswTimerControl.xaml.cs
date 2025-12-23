using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress.Wpf;
/// <summary>
/// TextBlock-based timer that can count up or down and renders its text directly (no template).
/// Supports formatting, start/end times, auto interval selection and optional Command on finish.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTimerControl StartTime="01:00:00" EndTime="00:00:00" IsCountingDown="True" Format="hh\:mm\:ss" IsRunning="True"/&gt;
/// </code>
/// </example>
public class StswTimerControl : TextBlock, ICommandSource
{
    static StswTimerControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimerControl), new FrameworkPropertyMetadata(typeof(StswTimerControl)));
    }
    public StswTimerControl()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        UpdateTimerInterval();
        UpdateDisplayText();
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets the command to be executed when the timer reaches the end time.
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
    /// Gets or sets the parameter to be passed to the command when it is executed.
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
    /// Gets or sets the target element on which the command is executed.
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
            typeof(StswTimerControl),
            new FrameworkPropertyMetadata(default(TimeSpan), OnCurrentTimeChanged)
        );
    private static void OnCurrentTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswTimerControl)d;
        stsw.UpdateDisplayText();
    }

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
            new FrameworkPropertyMetadata("mm\\:ss", OnFormatChanged)
        );
    public static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswTimerControl)d;
        stsw.UpdateTimerInterval();
        stsw.UpdateDisplayText();
        stsw.RestartTimerIfRunning();
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
        var stsw = (StswTimerControl)d;

        if ((bool)e.NewValue)
            stsw.StartInternal();
        else
            stsw._timer.Stop();
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

    #region Template
    private readonly DispatcherTimer _timer = new();
    private bool _isTimerTickSubscribed;

    /// <summary>
    /// Handles the Loaded event of the control, starting the timer if it is set to run.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isTimerTickSubscribed)
            return;

        _timer.Tick += Timer_Tick;
        _isTimerTickSubscribed = true;
    }

    /// <summary>
    /// Subscribes to the timer's tick event if not already subscribed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();

        if (!_isTimerTickSubscribed)
            return;

        _timer.Tick -= Timer_Tick;
        _isTimerTickSubscribed = false;
    }
    #endregion

    #region Logic
    private DateTime _runStartedUtc;
    private TimeSpan _baseTime;
    private TimeSpan _effectiveEndTime;

    /// <summary>
    /// Handles the Tick event of the timer, updating the current time and checking for completion.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!IsRunning)
            return;

        var elapsed = DateTime.UtcNow - _runStartedUtc;

        TimeSpan next;
        bool finished;

        if (IsCountingDown)
        {
            next = _baseTime - elapsed;
            if (next <= _effectiveEndTime)
            {
                next = _effectiveEndTime;
                finished = true;
            }
            else finished = false;
        }
        else
        {
            next = _baseTime + elapsed;
            if (next >= _effectiveEndTime)
            {
                next = _effectiveEndTime;
                finished = true;
            }
            else finished = false;
        }

        CurrentTime = next;

        if (finished)
            Finish();
    }

    /// <summary>
    /// Determines whether the timer can run based on the start and end times.
    /// </summary>
    /// <param name="effectiveEnd">The effective end time for the timer.</param>
    /// <returns><see langword="true"/> if the timer can run; otherwise, <see langword="false"/>.</returns>
    private bool CanRun(out TimeSpan effectiveEnd)
    {
        effectiveEnd = EndTime;
        return IsCountingDown ? StartTime > effectiveEnd : effectiveEnd > StartTime;
    }

    /// <summary>
    /// Updates the display text of the timer based on the current time and format.
    /// </summary>
    private void Finish()
    {
        SetCurrentValue(IsRunningProperty, false);
        _timer.Stop();

        switch (Command)
        {
            case RoutedCommand routed when routed.CanExecute(CommandParameter, CommandTarget ?? this):
                routed.Execute(CommandParameter, CommandTarget ?? this);
                break;
            case ICommand cmd when cmd.CanExecute(CommandParameter):
                cmd.Execute(CommandParameter);
                break;
        }
    }

    /// <summary>
    /// Restarts the timer if it is currently running.
    /// </summary>
    private void RestartTimerIfRunning()
    {
        if (!IsRunning)
            return;

        _timer.Stop();
        _timer.Start();
    }

    /// <summary>
    /// Starts the timer internally, initializing the base time and effective end time.
    /// </summary>
    private void StartInternal()
    {
        if (!CanRun(out var effEnd))
        {
            SetCurrentValue(IsRunningProperty, false);
            return;
        }

        _baseTime = StartTime;
        _effectiveEndTime = effEnd;

        if (ResetOnStart)
            CurrentTime = StartTime;

        _runStartedUtc = DateTime.UtcNow;
        _timer.Start();
    }

    /// <summary>
    /// Updates the display text of the timer based on the current time and format.
    /// </summary>
    private void UpdateDisplayText()
    {
        var fmt = Format;
        Text = string.IsNullOrWhiteSpace(fmt)
            ? CurrentTime.ToString()
            : CurrentTime.ToString(fmt);
    }

    /// <summary>
    /// Adjusts the timer's interval based on the <see cref="Format"/>.
    /// The interval is dynamically set to match the required precision, such as milliseconds, seconds, or minutes.
    /// </summary>
    private void UpdateTimerInterval()
    {
        var fmt = Format ?? string.Empty;
        _timer.Interval = fmt switch
        {
            _ when fmt.Contains("fff", StringComparison.Ordinal) => TimeSpan.FromMilliseconds(16),
            _ when fmt.Contains("ff", StringComparison.Ordinal) => TimeSpan.FromMilliseconds(20),
            _ when fmt.Contains('f') => TimeSpan.FromMilliseconds(100),
            _ when fmt.Contains("ss", StringComparison.Ordinal) => TimeSpan.FromMilliseconds(250),
            _ when fmt.Contains("mm", StringComparison.Ordinal) => TimeSpan.FromSeconds(1),
            _ when fmt.Contains("hh", StringComparison.Ordinal) => TimeSpan.FromSeconds(1),
            _ => TimeSpan.FromMilliseconds(250),
        };
    }
    #endregion
}
