using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A label control that supports icons, commands, and busy state indication.
/// Designed for use as a label for buttons, expanders, and other controls.
/// </summary>
/// <remarks>
/// This control extends <see cref="Label"/> with additional functionality, including icon support, 
/// command integration, and the ability to indicate a busy state.
/// </remarks>
[StswInfo(null)]
public class StswLabel : Label, IStswCornerControl, IStswIconControl
{
    static StswLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLabel), new FrameworkPropertyMetadata(typeof(StswLabel)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        Loaded += OnLoaded;
    }

    /// <summary>
    /// Handles the <see cref="FrameworkElement.Loaded"/> event.
    /// Ensures that the <see cref="IsBusy"/> property is bound to the <see cref="IsBusy"/> state of an associated command, if available.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control itself.</param>
    /// <param name="e">The event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (GetValue(IsBusyProperty) == null)
        {
            var commandSource = Parent as ICommandSource ?? TemplatedParent as ICommandSource;
            if (commandSource?.Command is IStswAsyncCommand cmd)
                SetBinding(IsBusyProperty, new Binding(nameof(IStswAsyncCommand.IsBusy)) { Source = cmd });
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the asynchronous command associated with the label.
    /// </summary>
    [StswInfo("0.14.0")]
    public IStswAsyncCommand Command
    {
        get => (IStswAsyncCommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.Register(
            nameof(Command),
            typeof(IStswAsyncCommand),
            typeof(StswLabel)
        );

    /// <inheritdoc/>
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(Geometry?),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public GridLength IconScale
    {
        get => (GridLength)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(GridLength),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the image source for the icon.
    /// </summary>
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(ImageSource?),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the label is in a busy state.
    /// When <see langword="true"/>, the label visually indicates that an associated operation is in progress.
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool?),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(bool?),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the content within the control is visible.
    /// When <see langword="false"/>, the content is hidden but the control remains in the layout.
    /// </summary>
    public bool IsContentVisible
    {
        get => (bool)GetValue(IsContentVisibleProperty);
        set => SetValue(IsContentVisibleProperty, value);
    }
    public static readonly DependencyProperty IsContentVisibleProperty
        = DependencyProperty.Register(
            nameof(IsContentVisible),
            typeof(bool),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the orientation of the label.
    /// Defines whether the icon and text are arranged horizontally or vertically.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(Orientation),
                FrameworkPropertyMetadataOptions.AffectsArrange)
        );
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
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(CornerRadius),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(Brush),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    [StswInfo("0.1.0")]
    public Brush IconStroke
    {
        get => (Brush)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }
    public static readonly DependencyProperty IconStrokeProperty
        = DependencyProperty.Register(
            nameof(IconStroke),
            typeof(Brush),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(Brush),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    [StswInfo("0.1.0")]
    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty IconStrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(IconStrokeThickness),
            typeof(double),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(double),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the text trimming behavior for the label.
    /// Defines how the text is trimmed when it overflows the available space.
    /// </summary>
    [StswInfo("0.16.0")]
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly DependencyProperty TextTrimmingProperty
        = DependencyProperty.Register(
            nameof(TextTrimming),
            typeof(TextTrimming),
            typeof(StswLabel),
            new FrameworkPropertyMetadata(default(TextTrimming),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswLabel Content="Download" IconData="{StaticResource DownloadIcon}" IsBusy="True"/>

*/
