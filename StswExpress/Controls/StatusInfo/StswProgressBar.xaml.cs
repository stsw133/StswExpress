using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A progress bar control with additional display options such as text representation and different states.
/// Supports percentage, value, and custom text display modes.
/// </summary>
/// <remarks>
/// This control provides a flexible progress visualization, allowing the display of progress in percentage,
/// absolute values, or custom text.
/// </remarks>
[Stsw(null, Changes = StswPlannedChanges.None)]
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
    /// Handles changes to the progress text based on the current value and display mode.
    /// Updates the <see cref="Text"/> property dynamically.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
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

    #region Logic properties
    /// <summary>
    /// Gets or sets the current state of the progress bar, which can be used for styling purposes.
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
    /// Gets or sets the text displayed on the progress bar.
    /// Updates dynamically based on the selected <see cref="TextMode"/>.
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
    /// Gets or sets the mode used to display progress text.
    /// Determines whether the progress bar shows a percentage, an absolute value, or custom text.
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
        if (obj is not StswProgressBar stsw)
            return;

        if (stsw.TextMode == StswProgressTextMode.Custom)
            stsw.Text = string.Empty;
    }
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
            typeof(StswProgressBar),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswProgressBar),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the fill brush used for the progress bar's visual representation.
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
            typeof(StswProgressBar),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswProgressBar Value="50" Minimum="0" Maximum="100" TextMode="Percentage"/>

*/
