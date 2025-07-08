using System;
using System.ComponentModel;
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
[Stsw("0.4.0", Changes = StswPlannedChanges.None)]
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

    /// <summary>
    /// Handles changes to the progress value and updates the stroke dash array
    /// to reflect the correct progress visualization in the ring.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnValueChanged(object? sender, EventArgs e) => StrokeDashArray = [(Value - Minimum) / (Maximum - Minimum) * 21.89204, 21.89204];
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
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswProgressRing stsw)
            return;

        stsw.Height = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
        stsw.Width = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
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
    public static void OnTextModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswProgressRing stsw)
            return;

        if (stsw.TextMode == StswProgressTextMode.Custom)
            stsw.Text = string.Empty;
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

/* usage:

<se:StswProgressRing Value="25" Minimum="0" Maximum="50" Scale="2"/>

*/
