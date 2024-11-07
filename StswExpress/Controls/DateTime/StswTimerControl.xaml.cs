using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// Represents a customizable timer control that can count up or down,
/// display the elapsed time in various formats, and provides properties
/// for controlling the start time, end time, and running state of the timer.
/// </summary>
[ContentProperty(nameof(EndTime))]
public class StswTimerControl : Control
{
    private readonly DispatcherTimer _timer = new();
    private DateTime _lastTickTime;

    public StswTimerControl()
    {
        _timer.Tick += Timer_Tick;
    }
    static StswTimerControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimerControl), new FrameworkPropertyMetadata(typeof(StswTimerControl)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswTimerControl), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private TextBlock? _display;
    
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        if (GetTemplateChild("PART_Display") is TextBlock display)
            _display = display;

        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Handles the tick event of the timer, updating the current time based on the elapsed time 
    /// since the last tick, and stops the timer when the end time is reached.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var elapsed = now - _lastTickTime;
        _lastTickTime = now;

        if (IsCountingDown)
        {
            CurrentTime -= elapsed;
            if (CurrentTime <= EndTime || CurrentTime <= TimeSpan.Zero)
            {
                CurrentTime = EndTime;
                IsRunning = false;
                _timer.Stop();
            }
        }
        else
        {
            CurrentTime += elapsed;
            if (CurrentTime >= EndTime)
            {
                CurrentTime = EndTime;
                IsRunning = false;
                _timer.Stop();
            }
        }
    }

    /// <summary>
    /// Updates the timer's interval based on the current format string. The interval is adjusted to match
    /// the precision required by the format, such as milliseconds or seconds.
    /// </summary>
    private void UpdateTimerInterval()
    {
        if (Format == null)
            _timer.Interval = TimeSpan.FromSeconds(1);
        else if (Format.Contains("fff"))
            _timer.Interval = TimeSpan.FromMilliseconds(1);
        else if (Format.Contains("ff"))
            _timer.Interval = TimeSpan.FromMilliseconds(10);
        else if (Format.Contains("f"))
            _timer.Interval = TimeSpan.FromMilliseconds(100);
        else if (Format.Contains("ss"))
            _timer.Interval = TimeSpan.FromSeconds(1);
        else if (Format.Contains("mm"))
            _timer.Interval = TimeSpan.FromMinutes(1);
        else if (Format.Contains("hh"))
            _timer.Interval = TimeSpan.FromHours(1);
        else
            _timer.Interval = TimeSpan.FromSeconds(1);
    }

    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the current time displayed by the timer. This value is updated during each tick event
    /// based on whether the timer is counting up or down.
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
    /// Gets or sets the end time for the timer. When the timer is counting down, this represents the target time to reach.
    /// When the timer is counting up, it represents the maximum time allowed.
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
    /// Gets or sets the format string used to display the current time. This string controls the precision 
    /// and appearance of the time display, such as including seconds, milliseconds, etc.
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
    public static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTimerControl stsw)
        {
            stsw._display?.SetBinding(TextBlock.TextProperty, new Binding(nameof(CurrentTime))
            {
                Source = stsw,
                StringFormat = stsw.Format
            });
            stsw.UpdateTimerInterval();
        }
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
    /// Gets or sets a value indicating whether the timer is currently running. When set to <see langword="true"/>, the timer starts 
    /// counting. When set to <see langword="false"/>, the timer stops. This property is typically controlled internally by the control.
    /// </summary>
    public bool IsRunning
    {
        get => (bool)GetValue(SelectedTimeProperty);
        private set => SetValue(SelectedTimeProperty, value);
    }
    public static readonly DependencyProperty SelectedTimeProperty
        = DependencyProperty.Register(
            nameof(IsRunning),
            typeof(bool),
            typeof(StswTimerControl),
            new FrameworkPropertyMetadata(default(bool), OnIsRunningChanged)
        );
    public static void OnIsRunningChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTimerControl stsw)
        {
            if ((bool)e.NewValue)
            {
                stsw._lastTickTime = DateTime.Now;
                stsw._timer.Start();
            }
            else
                stsw._timer.Stop();
        }
    }

    /// <summary>
    /// Gets or sets a value that controls the start, stop, and reset behavior of the timer. 
    /// When set to <see langword="true"/>, the timer starts or continues. When set to <see langword="false"/>, the timer stops. 
    /// When set to <see langword="null"/>, the timer resets to the start time.
    /// </summary>
    public bool? StartStopReset
    {
        get => (bool?)GetValue(StartStopResetProperty);
        set => SetValue(StartStopResetProperty, value);
    }
    public static readonly DependencyProperty StartStopResetProperty
        = DependencyProperty.Register(
            nameof(StartStopReset),
            typeof(bool?),
            typeof(StswTimerControl),
            new FrameworkPropertyMetadata(default(bool?), OnStartStopResetChanged)
        );
    public static void OnStartStopResetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTimerControl stsw)
        {
            var newValue = (bool?)e.NewValue;
            if (newValue == true)
            {
                stsw.CurrentTime = stsw.StartTime;
                stsw.IsRunning = true;
            }
            else if (newValue == false)
            {
                stsw.IsRunning = false;
            }
            else if (newValue == null)
            {
                stsw.IsRunning = false;
                stsw.CurrentTime = stsw.StartTime;
            }
        }
    }

    /// <summary>
    /// Gets or sets the start time for the timer. This value is used as the initial time from which the timer begins counting.
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
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswTimerControl),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswTimerControl),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
