using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress;
/// <summary>
/// A control for displaying vector-based icons.
/// Supports scaling, rotation, stroke thickness, and color customization.
/// </summary>
/// <remarks>
/// The control allows for various transformations, including scaling and rotation, making it a versatile choice for UI design.
/// </remarks>
[ContentProperty(nameof(Data))]
public class StswIcon : Control
{
    static StswIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIcon), new FrameworkPropertyMetadata(typeof(StswIcon)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswIcon), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        AssignAnimations();
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the canvas size of the icon.
    /// This determines the width and height of the drawing area for the icon.
    /// </summary>
    public double CanvasSize
    {
        get => (double)GetValue(CanvasSizeProperty);
        set => SetValue(CanvasSizeProperty, value);
    }
    public static readonly DependencyProperty CanvasSizeProperty
        = DependencyProperty.Register(
            nameof(CanvasSize),
            typeof(double),
            typeof(StswIcon),
            new FrameworkPropertyMetadata(24.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Gets or sets the geometry data of the icon.
    /// This defines the vector path used to render the icon.
    /// </summary>
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(
            nameof(Data),
            typeof(Geometry),
            typeof(StswIcon),
            new FrameworkPropertyMetadata(default(Geometry?),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the icon is rotated.
    /// When set to <see langword="true"/>, the icon rotates <c>180</c> degrees; otherwise, it resets to <c>0</c> degrees.
    /// </summary>
    public bool IsRotated
    {
        get => (bool)GetValue(IsRotatedProperty);
        set => SetValue(IsRotatedProperty, value);
    }
    public static readonly DependencyProperty IsRotatedProperty
        = DependencyProperty.Register(
            nameof(IsRotated),
            typeof(bool),
            typeof(StswIcon),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// The scale adjusts the icon's dimensions relative to its default size.
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
            typeof(StswIcon),
            new FrameworkPropertyMetadata(default(GridLength),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswIcon stsw)
        {
            stsw.Height = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
            stsw.Width = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush of the icon.
    /// This brush is used to paint the interior of the icon's geometry.
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
            typeof(StswIcon),
            new FrameworkPropertyMetadata(default(Brush),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the stroke brush of the icon.
    /// This brush is used to paint the outline of the icon's geometry.
    /// </summary>
    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }
    public static readonly DependencyProperty StrokeProperty
        = DependencyProperty.Register(
            nameof(Stroke),
            typeof(Brush),
            typeof(StswIcon),
            new FrameworkPropertyMetadata(default(Brush),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the icon's stroke.
    /// Determines the width of the outline drawn around the icon.
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty StrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(StswIcon),
            new FrameworkPropertyMetadata(default(double),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:
    
    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? BorderBrush { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Thickness? BorderThickness { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? Foreground { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new FontFamily? FontFamily { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new double FontSize { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new FontStretch FontStretch { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new FontWeight FontWeight { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new HorizontalAlignment HorizontalContentAlignment { get; private set; }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new VerticalAlignment VerticalContentAlignment { get; private set; }
    #endregion

    #region Animations
    /// <summary>
    /// Configures animations for the control, including rotation effects.
    /// Animations are triggered when the <see cref="IsRotated"/> property changes.
    /// </summary>
    public void AssignAnimations()
    {
        if (GetTemplateChild("PART_Viewbox") is not Viewbox viewBox)
            return;

        var expandAnimation = new DoubleAnimation
        {
            To = 180,
            Duration = TimeSpan.FromSeconds(1),
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new ElasticEase { Springiness = 9 }
        };

        var collapseAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(1),
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new ElasticEase { Springiness = 9 }
        };

        var expandStoryboard = new Storyboard();
        expandStoryboard.Children.Add(expandAnimation);
        Storyboard.SetTarget(expandAnimation, viewBox);
        Storyboard.SetTargetProperty(expandAnimation, new PropertyPath("(Viewbox.RenderTransform).(RotateTransform.Angle)"));

        var collapseStoryboard = new Storyboard();
        collapseStoryboard.Children.Add(collapseAnimation);
        Storyboard.SetTarget(collapseAnimation, viewBox);
        Storyboard.SetTargetProperty(collapseAnimation, new PropertyPath("(Viewbox.RenderTransform).(RotateTransform.Angle)"));

        DependencyPropertyDescriptor.FromProperty(IsRotatedProperty, typeof(StswIcon))
            ?.AddValueChanged(this, (s, e) =>
            {
                if (StswSettings.Default.EnableAnimations && StswControl.GetEnableAnimations(this))
                {
                    if (IsRotated)
                        expandStoryboard.Begin();
                    else
                        collapseStoryboard.Begin();
                }
                else
                {
                    expandStoryboard.Remove();
                    collapseStoryboard.Remove();
                    ((RotateTransform)viewBox.RenderTransform).Angle = IsRotated ? 180 : 0;
                }
            });
    }
    #endregion
}

/* usage:

<se:StswIcon Data="{StaticResource HomeIcon}" Fill="Blue" Stroke="Black" StrokeThickness="1"/>

*/

/// <summary>
/// Provides attached properties to control the behavior and appearance of a drop-down arrow.
/// Enables customization of the arrow's visibility, rotation, and geometry.
/// </summary>
public static class StswDropArrow
{
    /// <summary>
    /// Gets or sets the geometry data for the drop-down arrow.
    /// Defines the vector shape of the arrow.
    /// </summary>
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.RegisterAttached(
            nameof(DataProperty)[..^8],
            typeof(Geometry),
            typeof(StswDropArrow),
            new PropertyMetadata(default(Geometry), OnPropertyChanged)
        );
    public static void SetData(DependencyObject obj, Geometry value) => obj.SetValue(DataProperty, value);

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down arrow is rotated.
    /// When set to <see langword="true"/>, the arrow is rotated to indicate an expanded state.
    /// </summary>
    public static readonly DependencyProperty IsRotatedProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsRotatedProperty)[..^8],
            typeof(bool),
            typeof(StswDropArrow),
            new PropertyMetadata(default(bool), OnPropertyChanged)
        );
    public static void SetIsRotated(DependencyObject obj, bool value) => obj.SetValue(IsRotatedProperty, value);

    /// <summary>
    /// Identifies the <see cref="Visibility"/> attached property.
    /// When set to <see cref="Visibility.Collapsed"/>, the drop-down arrow is hidden.
    /// </summary>
    public static readonly DependencyProperty VisibilityProperty
        = DependencyProperty.RegisterAttached(
            nameof(VisibilityProperty)[..^8],
            typeof(Visibility),
            typeof(StswDropArrow),
            new PropertyMetadata(default(Visibility), OnPropertyChanged)
        );
    public static void SetVisibility(DependencyObject obj, Visibility value) => obj.SetValue(VisibilityProperty, value);

    /// <summary>
    /// Applies changes to the drop-down arrow when a property value changes.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <param name="e">The event arguments containing the changed property.</param>
    private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is Control control)
        {
            EnsureTemplate(control);
        }
    }

    /// <summary>
    /// Ensures that the control has a template before applying drop-arrow properties.
    /// </summary>
    /// <param name="control">The control instance to check.</param>
    private static void EnsureTemplate(Control control)
    {
        if (control.Template == null)
        {
            if (!control.IsLoaded)
                control.Loaded += Control_Loaded;
            return;
        }

        ApplyDropArrowProperties(control);
        TrackPropertyChanges(control);
    }

    /// <summary>
    /// Applies drop-arrow properties such as geometry, rotation, and visibility to the target control.
    /// </summary>
    /// <param name="control">The control to update.</param>
    private static void ApplyDropArrowProperties(Control control)
    {
        if (control.Template?.FindName("OPT_DropArrow", control) is StswIcon dropArrow)
        {
            dropArrow.Data = (Geometry)control.GetValue(DataProperty);
            dropArrow.IsRotated = (bool)control.GetValue(IsRotatedProperty);
            dropArrow.Visibility = (Visibility)control.GetValue(VisibilityProperty);
        }
    }

    /// <summary>
    /// Handles the <see cref="FrameworkElement.Loaded"/> event to apply drop-arrow settings once the control is fully loaded.
    /// </summary>
    /// <param name="sender">The control that triggered the event.</param>
    /// <param name="e">Event data.</param>
    private static void Control_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Control control)
        {
            control.Loaded -= Control_Loaded;
            EnsureTemplate(control);
        }
    }

    /// <summary>
    /// Tracks changes to drop-arrow-related properties and updates the control accordingly.
    /// </summary>
    /// <param name="control">The control to monitor for property changes.</param>
    private static void TrackPropertyChanges(Control control)
    {
        DependencyPropertyDescriptor.FromProperty(DataProperty, typeof(StswDropArrow))?.AddValueChanged(control, (s, e) => ApplyDropArrowProperties(control));
        DependencyPropertyDescriptor.FromProperty(IsRotatedProperty, typeof(StswDropArrow))?.AddValueChanged(control, (s, e) => ApplyDropArrowProperties(control));
        DependencyPropertyDescriptor.FromProperty(VisibilityProperty, typeof(StswDropArrow))?.AddValueChanged(control, (s, e) => ApplyDropArrowProperties(control));
    }
}
