using System.Windows.Controls;
using System.Windows;
using System.Timers;
using System;

namespace StswExpress;
/// <summary>
/// Represents a control that manages the display of different content for a specified duration when the timer is enabled.
/// </summary>
public class StswTimedSwitch : CheckBox
{
    static StswTimedSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimedSwitch), new FrameworkPropertyMetadata(typeof(StswTimedSwitch)));
    }

    #region Events & methods
    private readonly Timer timer = new() { AutoReset = false };

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        timer.Elapsed += Timer_Elapsed;

        OnSwitchTimeChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        if (SwitchTime.TotalMilliseconds > 0)
            timer.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        timer.Stop();
    }

    /// <summary>
    /// Handles the Elapsed event of the Timer to switch back to the default content.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
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
    /// Gets or sets the duration for how long the content is switched from default content to timed content.
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
        if (obj is StswTimedSwitch stsw)
        {
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
    }

    /// <summary>
    /// Gets or sets the content to be displayed for a specific duration after enabling the timer.
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
    /// Gets or sets a composite string format that specifies how to format the timed content if it is displayed as a string.
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
    /// Gets or sets the data template used to display the timed content.
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
    /// Gets or sets the custom logic for choosing the template used to display the timed content.
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
