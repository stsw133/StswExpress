using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StswExpress;
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
    private readonly DispatcherTimer timer = new();

    public StswTimedSwitch()
    {
        timer.Tick += Timer_Tick;
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
        timer.Stop();
    }

    /// <summary>
    /// Handles the timer tick event, reverting the switch to its default state after the specified duration.
    /// </summary>
    /// <param name="sender">The timer triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        timer.Stop();

        if (IsChecked == true)
            SetCurrentValue(IsCheckedProperty, false);
    }

    /// <summary>
    /// Starts the timer to revert the switch after the specified <see cref="SwitchTime"/>.
    /// </summary>
    private void StartTimer()
    {
        if (SwitchTime <= TimeSpan.Zero)
            return;

        timer.Stop();
        timer.Interval = SwitchTime;
        timer.Start();
    }

    /// <summary>
    /// Updates the timer interval when the <see cref="SwitchTime"/> property changes.
    /// </summary>
    private void UpdateTimerAfterSwitchTimeChange()
    {
        if (SwitchTime <= TimeSpan.Zero)
        {
            timer.Stop();
            return;
        }

        timer.Interval = SwitchTime;

        if (IsChecked == true)
        {
            timer.Stop();
            timer.Start();
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
