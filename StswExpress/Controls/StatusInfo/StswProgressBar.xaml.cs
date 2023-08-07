using System.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control being a progress bar with additional features such as displaying progress as text and different states.
/// </summary>
public class StswProgressBar : ProgressBar
{
    public StswProgressBar()
    {
        OnTextChanged(this, EventArgs.Empty);
        DependencyPropertyDescriptor.FromProperty(MaximumProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(MinimumProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(TextModeProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
        DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(ProgressBar)).AddValueChanged(this, OnTextChanged);
    }
    static StswProgressBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswProgressBar), new FrameworkPropertyMetadata(typeof(StswProgressBar)));
    }

    #region Events & methods
    /// <summary>
    /// Event handler to update the text displayed on the progress bar based on its state.
    /// </summary>
    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (Maximum != Minimum)
        {
            if (TextMode == StswProgressTextMode.Percentage)
            {
                Text = $"{(int)((Value - Minimum) / (Maximum - Minimum) * 100)} %";
                return;
            }
            else if (TextMode == StswProgressTextMode.Value)
            {
                Text = $"{Value - Minimum} / {Maximum - Minimum}";
                return;
            }
        }
        Text = string.Empty;
    }
    #endregion

    #region Main properties
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
            typeof(StswProgressBar)
        );

    /// <summary>
    /// Gets the text to display on the progress bar indicating the progress status.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        private set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswProgressBar)
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
            typeof(StswProgressBar)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswProgressBar)
        );
    #endregion
}
