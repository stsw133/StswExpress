using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A circular progress indicator with text and scaling support.
/// Can display percentage, value, or custom text inside the ring.
/// </summary>
/// <remarks>
/// This control provides a visual representation of progress in a ring format.
/// It supports different text display modes and scaling for various UI requirements.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswProgressRing Value="25" Minimum="0" Maximum="50" Scale="2"/&gt;
/// </code>
/// </example>
public class StswProgressRing : ProgressBar
{
    public StswProgressRing()
    {
        SetCurrentValue(StrokeDashArrayProperty, _strokeDashArray);
        UpdateProgressText();
        UpdateStrokeDashArray();
    }
    static StswProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswProgressRing), new FrameworkPropertyMetadata(typeof(StswProgressRing)));
    }

    #region Events & methods
    /// <inheritdoc />
    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        UpdateProgressText();
        UpdateStrokeDashArray();
    }

    /// <inheritdoc />
    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        UpdateProgressText();
        UpdateStrokeDashArray();
    }

    /// <inheritdoc />
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateProgressText();
        UpdateStrokeDashArray();
    }

    /// <summary>
    /// Updates the progress text based on the current value and selected text mode.
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

    /// <summary>
    /// Updates the stroke dash array to reflect the current progress value.
    /// </summary>
    private void UpdateStrokeDashArray()
    {
        if (Maximum <= Minimum)
        {
            _strokeDashArray[0] = 0d;
            _strokeDashArray[1] = StrokeDashLength;
        }
        else
        {
            var normalized = Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0d, 1d);

            _strokeDashArray[0] = normalized * StrokeDashLength;
            _strokeDashArray[1] = StrokeDashLength;
        }

        SetCurrentValue(StrokeDashArrayProperty, null);
        SetCurrentValue(StrokeDashArrayProperty, _strokeDashArray);
    }

    private const double StrokeDashLength = 21.89204;
    private readonly DoubleCollection _strokeDashArray = [0d, StrokeDashLength];
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the scale of the progress ring.
    /// Determines the size of the ring in proportion to its default dimensions.
    /// </summary>
    public GridLength Scale
    {
        get => (GridLength)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(StswProgressRing),
            new FrameworkPropertyMetadata(default(GridLength),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswProgressRing stsw)
            return;

        IStswIconControl.ScaleChanged(stsw, stsw.Scale);
    }

    /// <summary>
    /// Gets or sets the current state of the progress ring, which can be used for styling purposes.
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
    /// Gets or sets the stroke dash array used to control the visibility of the ring's progress arc.
    /// This property dynamically updates based on the <see cref="Value"/>.
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
    /// Gets or sets the text displayed inside the progress ring.
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
            typeof(StswProgressRing)
        );

    /// <summary>
    /// Gets or sets the mode used to display progress text.
    /// Determines whether the progress ring shows a percentage, an absolute value, or custom text.
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
    public static void OnTextModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswProgressRing stsw)
            return;

        if (stsw.TextMode == StswProgressTextMode.Custom)
            stsw.SetCurrentValue(TextProperty, string.Empty);
        else
            stsw.UpdateProgressText();
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush used for the progress ring's visual representation.
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
            typeof(StswProgressRing),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
