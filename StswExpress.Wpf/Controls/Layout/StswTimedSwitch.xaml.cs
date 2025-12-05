using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StswExpress.Wpf;
/// <summary>
/// A switch control that automatically reverts to its default state after a specified duration.
/// Supports custom content display during the active period.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTimedSwitch Content="Activate" TimedContent="Activated" SwitchTime="00:00:05"/&gt;
/// </code>
/// </example>
public class StswTimedSwitch : CheckBox
{
    private readonly DispatcherTimer _timer = new();
    private bool _isTimerTickSubscribed;

    public StswTimedSwitch()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SubscribeToTimerTick();
    }
    static StswTimedSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimedSwitch), new FrameworkPropertyMetadata(typeof(StswTimedSwitch)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        StartTimer();
    }

    /// <inheritdoc/>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        _timer.Stop();
    }

    /// <summary>
    /// Handles the timer tick event, reverting the switch to its default state after the specified duration.
    /// </summary>
    /// <param name="sender">The timer triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        _timer.Stop();

        if (IsChecked == true)
            SetCurrentValue(IsCheckedProperty, false);
    }

    /// <summary>
    /// Handles the Loaded event to subscribe to the timer tick event.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnLoaded(object sender, RoutedEventArgs e) => SubscribeToTimerTick();

    /// <summary>
    /// Handles the Unloaded event to stop the timer and unsubscribe from the tick event.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        UnsubscribeFromTimerTick();
    }

    /// <summary>
    /// Subscribes to the timer tick event.
    /// </summary>
    private void SubscribeToTimerTick()
    {
        if (_isTimerTickSubscribed)
            return;

        _timer.Tick += Timer_Tick;
        _isTimerTickSubscribed = true;
    }

    /// <summary>
    /// Unsubscribes from the timer tick event to prevent memory leaks.
    /// </summary>
    private void UnsubscribeFromTimerTick()
    {
        if (!_isTimerTickSubscribed)
            return;

        _timer.Tick -= Timer_Tick;
        _isTimerTickSubscribed = false;
    }

    /// <summary>
    /// Starts the timer to revert the switch after the specified <see cref="SwitchTime"/>.
    /// </summary>
    private void StartTimer()
    {
        if (SwitchTime <= TimeSpan.Zero)
            return;

        _timer.Stop();
        _timer.Interval = SwitchTime;
        _timer.Start();
    }

    /// <summary>
    /// Updates the timer interval when the <see cref="SwitchTime"/> property changes.
    /// </summary>
    private void UpdateTimerAfterSwitchTimeChange()
    {
        if (SwitchTime <= TimeSpan.Zero)
        {
            _timer.Stop();
            return;
        }

        _timer.Interval = SwitchTime;

        if (IsChecked == true)
        {
            _timer.Stop();
            _timer.Start();
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the duration after which the switch automatically reverts to its default state.
    /// If set to zero, the switch remains in the active state indefinitely.
    /// </summary>
    public TimeSpan SwitchTime
    {
        get => (TimeSpan)GetValue(SwitchTimeProperty);
        set => SetValue(SwitchTimeProperty, value);
    }
    public static readonly DependencyProperty SwitchTimeProperty
        = DependencyProperty.Register(
            nameof(SwitchTime),
            typeof(TimeSpan),
            typeof(StswTimedSwitch),
            new PropertyMetadata(default(TimeSpan), OnSwitchTimeChanged)
        );
    public static void OnSwitchTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswTimedSwitch stsw)
            return;

        stsw.UpdateTimerAfterSwitchTimeChange();
    }

    /// <summary>
    /// Gets or sets the content displayed during the active state of the switch.
    /// Once the <see cref="SwitchTime"/> duration elapses, the content reverts to the default state.
    /// </summary>
    public object TimedContent
    {
        get => GetValue(TimedContentProperty);
        set => SetValue(TimedContentProperty, value);
    }
    public static readonly DependencyProperty TimedContentProperty
        = DependencyProperty.Register(
            nameof(TimedContent),
            typeof(object),
            typeof(StswTimedSwitch)
        );

    /// <summary>
    /// Gets or sets a format string applied to the <see cref="TimedContent"/> when displayed as text.
    /// </summary>
    public string TimedContentStringFormat
    {
        get => (string)GetValue(TimedContentStringFormatProperty);
        set => SetValue(TimedContentStringFormatProperty, value);
    }
    public static readonly DependencyProperty TimedContentStringFormatProperty
        = DependencyProperty.Register(
            nameof(TimedContentStringFormat),
            typeof(string),
            typeof(StswTimedSwitch)
        );

    /// <summary>
    /// Gets or sets the data template used to display the <see cref="TimedContent"/>.
    /// </summary>
    public DataTemplate TimedContentTemplate
    {
        get => (DataTemplate)GetValue(TimedContentTemplateProperty);
        set => SetValue(TimedContentTemplateProperty, value);
    }
    public static readonly DependencyProperty TimedContentTemplateProperty
        = DependencyProperty.Register(
            nameof(TimedContentTemplate),
            typeof(DataTemplate),
            typeof(StswTimedSwitch)
        );

    /// <summary>
    /// Gets or sets a template selector that determines which template to apply to the <see cref="TimedContent"/>.
    /// </summary>
    public DataTemplateSelector TimedContentTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(TimedContentTemplateSelectorProperty);
        set => SetValue(TimedContentTemplateSelectorProperty, value);
    }
    public static readonly DependencyProperty TimedContentTemplateSelectorProperty
        = DependencyProperty.Register(
            nameof(TimedContentTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(StswTimedSwitch)
        );
    #endregion
}
