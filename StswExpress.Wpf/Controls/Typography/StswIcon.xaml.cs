using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress.Wpf;
/// <summary>
/// A control for displaying vector-based icons.
/// Supports scaling, rotation, stroke thickness, and color customization.
/// </summary>
/// <remarks>
/// The control allows for various transformations, including scaling and rotation, making it a versatile choice for UI design.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswIcon Data="{StaticResource HomeIcon}" Fill="Blue" Stroke="Black" StrokeThickness="1"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Data))]
public class StswIcon : FrameworkElement
{
    static StswIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIcon), new FrameworkPropertyMetadata(typeof(StswIcon)));
    }
    public StswIcon()
    {
        _rotateTransform = new RotateTransform(0);
        RenderTransform = _rotateTransform;
        RenderTransformOrigin = new Point(0.5, 0.5);

        (_expandStoryboard, _collapseStoryboard) = CreateStoryboards();
        ApplyRotation(IsRotated, animate: false);
    }

    private readonly RotateTransform _rotateTransform;
    private readonly Storyboard _collapseStoryboard;
    private readonly Storyboard _expandStoryboard;

    #region Dependency properties
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
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender)
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
    /// Gets or sets the foreground brush of the icon.
    /// This value is inherited from parent controls and used as a fallback for <see cref="Fill"/>.
    /// </summary>
    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
    public static readonly DependencyProperty ForegroundProperty
        = Control.ForegroundProperty.AddOwner(
            typeof(StswIcon),
            new FrameworkPropertyMetadata(SystemColors.ControlTextBrush,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender)
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
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnIsRotatedChanged)
        );
    private static void OnIsRotatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswIcon)d;
        stsw.ApplyRotation((bool)e.NewValue, animate: true);
    }

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
    public static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswIcon)d;
        IStswIconControl.ScaleChanged(stsw, stsw.Scale);
    }

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
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnPenRelevantPropertyChanged)
        );
    private static void OnPenRelevantPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswIcon)d;
        stsw.InvalidatePenCache();
    }

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
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnPenRelevantPropertyChanged)
        );
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize) => finalSize;

    /// <inheritdoc/>
    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        var pt = hitTestParameters.HitPoint;
        if (pt.X >= 0 && pt.X <= ActualWidth
         && pt.Y >= 0 && pt.Y <= ActualHeight)
            return new PointHitTestResult(this, pt);

        return null;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var desiredWidth = double.IsNaN(Width) ? CanvasSize : Width;
        var desiredHeight = double.IsNaN(Height) ? CanvasSize : Height;

        if (!double.IsInfinity(availableSize.Width))
            desiredWidth = Math.Min(availableSize.Width, desiredWidth);

        if (!double.IsInfinity(availableSize.Height))
            desiredHeight = Math.Min(availableSize.Height, desiredHeight);

        return new(desiredWidth, desiredHeight);
    }

    /// <inheritdoc/>
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (Data == null || CanvasSize <= 0)
            return;

        var rect = new Rect(0, 0, ActualWidth, ActualHeight);
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        var scale = Math.Min(rect.Width / CanvasSize, rect.Height / CanvasSize);
        if (scale <= 0)
            return;

        var offsetX = (rect.Width - CanvasSize * scale) / 2;
        var offsetY = (rect.Height - CanvasSize * scale) / 2;

        drawingContext.PushTransform(new TranslateTransform(offsetX, offsetY));
        drawingContext.PushTransform(new ScaleTransform(scale, scale));

        var pen = GetPen();
        var fill = Fill ?? Foreground;
        drawingContext.DrawGeometry(fill, pen, Data);

        drawingContext.Pop();
        drawingContext.Pop();
    }
    #endregion

    #region Logic
    private Pen? _cachedPen;
    private Brush? _cachedStroke;
    private double _cachedStrokeThickness;

    /// <summary>
    /// Gets the cached pen or creates a new one if necessary.
    /// </summary>
    /// <returns>The pen to be used for stroking the icon.</returns>
    private Pen? GetPen()
    {
        if (StrokeThickness <= 0 || Stroke == null)
            return null;

        if (_cachedPen != null
         && ReferenceEquals(_cachedStroke, Stroke)
         && _cachedStrokeThickness.Equals(StrokeThickness))
            return _cachedPen;

        var pen = new Pen(Stroke, StrokeThickness);
        if (pen.CanFreeze)
            pen.Freeze();

        _cachedPen = pen;
        _cachedStroke = Stroke;
        _cachedStrokeThickness = StrokeThickness;

        return _cachedPen;
    }

    /// <summary>
    /// Invalidates the cached pen.
    /// </summary>
    private void InvalidatePenCache()
    {
        _cachedPen = null;
        _cachedStroke = null;
        _cachedStrokeThickness = 0;
    }
    #endregion

    #region Animations
    /// <summary>
    /// Creates the expand and collapse storyboards for rotation animations.
    /// </summary>
    /// <param name="rotate">Indicates whether to rotate the icon.</param>
    /// <param name="animate">Indicates whether to animate the rotation.</param>
    private void ApplyRotation(bool rotate, bool animate)
    {
        if (animate && StswApp.Settings.AnimationsEnabled && StswControl.GetEnableAnimations(this))
        {
            if (rotate)
            {
                _collapseStoryboard.Stop(this);
                _expandStoryboard.Begin(this, true);
            }
            else
            {
                _expandStoryboard.Stop(this);
                _collapseStoryboard.Begin(this, true);
            }
        }
        else
        {
            _expandStoryboard.Stop(this);
            _collapseStoryboard.Stop(this);
            _rotateTransform.Angle = rotate ? 180 : 0;
        }
    }

    /// <summary>
    /// Creates the expand and collapse storyboards for the rotation animations.
    /// </summary>
    /// <returns>A tuple containing the expand and collapse storyboards.</returns>
    private (Storyboard expandStoryboard, Storyboard collapseStoryboard) CreateStoryboards()
    {
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
        Storyboard.SetTarget(expandAnimation, this);
        Storyboard.SetTargetProperty(expandAnimation, new PropertyPath("(FrameworkElement.RenderTransform).(RotateTransform.Angle)"));

        var collapseStoryboard = new Storyboard();
        collapseStoryboard.Children.Add(collapseAnimation);
        Storyboard.SetTarget(collapseAnimation, this);
        Storyboard.SetTargetProperty(collapseAnimation, new PropertyPath("(FrameworkElement.RenderTransform).(RotateTransform.Angle)"));

        return (expandStoryboard, collapseStoryboard);
    }
    #endregion
}
