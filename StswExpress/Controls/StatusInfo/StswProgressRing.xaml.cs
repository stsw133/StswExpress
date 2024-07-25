using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a control being a progress bar with additional features such as displaying progress as text and different states.
/// </summary>
public class StswProgressRing : ProgressBar
{
    public StswProgressRing()
    {
        OnTextChanged(this, EventArgs.Empty);
        DependencyPropertyDescriptor.FromProperty(MaximumProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(MinimumProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(TextModeProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        OnValueChanged(this, EventArgs.Empty);
        DependencyPropertyDescriptor.FromProperty(MaximumProperty, typeof(ProgressBar)).AddValueChanged(this, OnValueChanged);
        DependencyPropertyDescriptor.FromProperty(MinimumProperty, typeof(ProgressBar)).AddValueChanged(this, OnValueChanged);
        DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(ProgressBar)).AddValueChanged(this, OnValueChanged);
    }
    static StswProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswProgressRing), new FrameworkPropertyMetadata(typeof(StswProgressRing)));
    }

    #region Events & methods
    /// <summary>
    /// Event handler to update the text displayed on the progress bar based on its state.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (Maximum != Minimum && TextMode != StswProgressTextMode.Custom)
        {
            Text = TextMode switch
            {
                StswProgressTextMode.None => string.Empty,
                StswProgressTextMode.Percentage => $"{(int)((Value - Minimum) / (Maximum - Minimum) * 100)} %",
                StswProgressTextMode.Progress => $"{Value - Minimum} / {Maximum - Minimum}",
                StswProgressTextMode.Value => $"{(int)Value}",
                _ => null
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnValueChanged(object? sender, EventArgs e) => StrokeDashArray = new DoubleCollection() { (Value - Minimum) / (Maximum - Minimum) * 34.4589, 34.4589 };
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the current state of the progress bar.
    /// </summary>
    public StswProgressState State
    {
        get => (StswProgressState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
    public static readonly DependencyProperty StateProperty
        = DependencyProperty.Register(
            nameof(State),
            typeof(StswProgressState),
            typeof(StswProgressRing)
        );

    /// <summary>
    /// 
    /// </summary>
    internal DoubleCollection StrokeDashArray
    {
        get => (DoubleCollection)GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }
    public static readonly DependencyProperty StrokeDashArrayProperty
        = DependencyProperty.Register(
            nameof(StrokeDashArray),
            typeof(DoubleCollection),
            typeof(StswProgressRing)
        );

    /// <summary>
    /// Gets the text to display on the progress bar indicating the progress status.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswProgressRing)
        );

    /// <summary>
    /// Gets or sets a value indicating whether to display the progress text in percents or the current value range.
    /// </summary>
    public StswProgressTextMode TextMode
    {
        get => (StswProgressTextMode)GetValue(TextModeProperty);
        set => SetValue(TextModeProperty, value);
    }
    public static readonly DependencyProperty TextModeProperty
        = DependencyProperty.Register(
            nameof(TextMode),
            typeof(StswProgressTextMode),
            typeof(StswProgressRing),
            new FrameworkPropertyMetadata(default(StswProgressTextMode),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextModeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnTextModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswProgressRing stsw)
        {
            if (stsw.TextMode == StswProgressTextMode.Custom)
                stsw.Text = string.Empty;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// 
    /// </summary>
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
    public static readonly DependencyProperty FillProperty
        = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(StswProgressRing)
        );
    #endregion
}
