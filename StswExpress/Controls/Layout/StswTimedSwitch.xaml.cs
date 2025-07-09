using System.Windows.Controls;
using System.Windows;
using System.Timers;
using System;

namespace StswExpress;
/// <summary>
/// A switch control that automatically reverts to its default state after a specified duration.
/// Supports custom content display during the active period.
/// </summary>
[Stsw("0.5.0")]
public class StswTimedSwitch : CheckBox
{
    private readonly Timer timer = new() { AutoReset = false };

    static StswTimedSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimedSwitch), new FrameworkPropertyMetadata(typeof(StswTimedSwitch)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        timer.Elapsed += Timer_Elapsed;

        OnSwitchTimeChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <inheritdoc/>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        if (SwitchTime.TotalMilliseconds > 0)
            timer.Start();
    }

    /// <inheritdoc/>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        timer.Stop();
    }

    /// <summary>
    /// Handles the timer's elapsed event, reverting the switch to its default state after the specified duration.
    /// </summary>
    /// <param name="sender">The timer triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (IsChecked == true)
            {
                SetValue(IsCheckedProperty, false);
                timer.Stop();
            }
        });
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
    public static void OnSwitchTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswTimedSwitch stsw)
            return;

        if (stsw.SwitchTime.TotalMilliseconds > 0)
        {
            var newInterval = stsw.SwitchTime.TotalMilliseconds;
            if (stsw.timer.Interval != newInterval)
            {
                stsw.timer.Interval = newInterval;
                if (stsw.IsChecked == true)
                    stsw.timer.Start();
            }
        }
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

/* usage:

<se:StswTimedSwitch Content="Activate" TimedContent="Activated" SwitchTime="00:00:05"/>

*/
