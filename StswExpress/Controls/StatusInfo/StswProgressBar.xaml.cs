using System;
using System.Globalization;
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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswProgressBar Value="50" Minimum="0" Maximum="100" TextMode="Percentage"/&gt;
/// </code>
/// </example>
public class StswProgressBar : ProgressBar, IStswCornerControl
{
    public StswProgressBar()
    {
        UpdateProgressText();
    }
    static StswProgressBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswProgressBar), new FrameworkPropertyMetadata(typeof(StswProgressBar)));
    }

    #region Events & methods
    /// <inheritdoc />
    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        UpdateProgressText();
    }

    /// <inheritdoc />
    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        UpdateProgressText();
    }

    /// <inheritdoc />
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateProgressText();
    }

    /// <summary>
    /// Updates the progress text based on the current value, minimum, maximum, and selected text mode.
    /// </summary>
    private void UpdateProgressText()
    {
        if (TextMode == StswProgressTextMode.Custom)
            return;

        if (Maximum <= Minimum)
        {
            SetCurrentValue(TextProperty, string.Empty);
            return;
        }

        var range = Maximum - Minimum;
        var current = Value - Minimum;
        var progress = Math.Clamp(current / range, 0d, 1d);

        var text = TextMode switch
        {
            StswProgressTextMode.None => string.Empty,
            StswProgressTextMode.Percentage => string.Format(CultureInfo.CurrentCulture, "{0} %", (int)(progress * 100)),
            StswProgressTextMode.Progress => string.Format(CultureInfo.CurrentCulture, "{0} / {1}", current.ToString(CultureInfo.CurrentCulture), range.ToString(CultureInfo.CurrentCulture)),
            StswProgressTextMode.Value => ((int)Value).ToString(CultureInfo.CurrentCulture),
            _ => null
        };

        SetCurrentValue(TextProperty, text);
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
    public static void OnTextModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswProgressBar stsw)
            return;

        if (stsw.TextMode == StswProgressTextMode.Custom)
            stsw.SetCurrentValue(TextProperty, string.Empty);
        else
            stsw.UpdateProgressText();
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
