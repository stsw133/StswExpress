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
public class StswProgressBar : ProgressBar, IStswCornerControl
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
        set => SetValue(TextProperty, value);
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
            typeof(StswProgressBar),
            new FrameworkPropertyMetadata(default(StswProgressTextMode),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextModeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnTextModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswProgressBar stsw)
        {
            if (stsw.TextMode == StswProgressTextMode.Custom)
                stsw.Text = string.Empty;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match the
    /// border's rounded corners, preventing elements from protruding beyond the border.
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
            typeof(StswProgressBar)
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
            typeof(StswProgressBar)
        );

    /// <summary>
    /// Gets or sets the fill brush for the control.
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
            typeof(StswProgressBar)
        );
    #endregion
}
