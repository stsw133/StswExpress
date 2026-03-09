using System;
using System.Windows;
using System.Windows.Media;

namespace StswExpress.Wpf;

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
public class StswProgressRing : StswProgressBar
{
    private const double GeometryCenter = 5d;
    private const double GeometryRadius = 4.5d;

    static StswProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswProgressRing), new FrameworkPropertyMetadata(typeof(StswProgressRing)));
    }
    public StswProgressRing()
    {
        UpdateProgressGeometry();
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets the geometry representing the progress arc.
    /// </summary>
    internal Geometry ProgressGeometry
    {
        get => (Geometry)GetValue(ProgressGeometryProperty);
        set => SetValue(ProgressGeometryProperty, value);
    }
    public static readonly DependencyProperty ProgressGeometryProperty
        = DependencyProperty.Register(
            nameof(ProgressGeometry),
            typeof(Geometry),
            typeof(StswProgressRing),
            new FrameworkPropertyMetadata(Geometry.Empty, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the progress ring stroke.
    /// </summary>
    public double RingThickness
    {
        get => (double)GetValue(RingThicknessProperty);
        set => SetValue(RingThicknessProperty, value);
    }
    public static readonly DependencyProperty RingThicknessProperty
        = DependencyProperty.Register(
            nameof(RingThickness),
            typeof(double),
            typeof(StswProgressRing),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the scale of the progress ring.
    /// Determines the size of the ring in proportion to its default dimensions.
    /// </summary>
    public GridLength? Scale
    {
        get => (GridLength?)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength?),
            typeof(StswProgressRing),
            new FrameworkPropertyMetadata(default(GridLength?),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswProgressRing)d;
        IStswIconControl.ScaleChanged(stsw, stsw.Scale);
    }
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        UpdateProgressGeometry();
    }

    /// <inheritdoc/>
    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        UpdateProgressGeometry();
    }

    /// <inheritdoc/>
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateProgressGeometry();
    }
    #endregion

    #region Logic
    /// <summary>
    /// Updates the geometry of the progress arc based on current Value/Minimum/Maximum.
    /// </summary>
    private void UpdateProgressGeometry()
    {
        if (IsIndeterminate)
            return;

        Geometry geometry;

        if (Maximum <= Minimum || double.IsNaN(Value) || double.IsInfinity(Value))
        {
            geometry = Geometry.Empty;
            _lastNormalized = double.NaN;
        }
        else
        {
            var normalized = Math.Clamp(
                (Value - Minimum) / (Maximum - Minimum),
                0d,
                1d);

            if (normalized.Equals(_lastNormalized))
                return;

            _lastNormalized = normalized;
            geometry = CreateArcGeometry(normalized);
        }

        ProgressGeometry = geometry;
    }
    private double _lastNormalized = double.NaN;

    /// <summary>
    /// Creates an arc geometry representing the progress based on the normalized value (0 to 1).
    /// </summary>
    private static Geometry CreateArcGeometry(double normalized)
    {
        if (normalized <= 0d)
            return Geometry.Empty;

        if (normalized >= 1d - 0.0001d)
            return FullCircleGeometry;

        var center = new Point(GeometryCenter, GeometryCenter);
        var sweepAngle = 360d * normalized;
        var startPoint = PointOnCircle(center, GeometryRadius, 0d);
        var endPoint = PointOnCircle(center, GeometryRadius, sweepAngle);
        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(startPoint, isFilled: false, isClosed: false);
            ctx.ArcTo(
                endPoint,
                new Size(GeometryRadius, GeometryRadius),
                rotationAngle: 0,
                isLargeArc: sweepAngle > 180d,
                sweepDirection: SweepDirection.Clockwise,
                isStroked: true,
                isSmoothJoin: false);
        }

        geometry.Freeze();
        return geometry;
    }

    /// <summary>
    /// Creates a full circle geometry.
    /// </summary>
    /// <returns>Geometry representing a full circle.</returns>
    private static StreamGeometry CreateFullCircleGeometry()
    {
        var center = new Point(GeometryCenter, GeometryCenter);
        var startPoint = PointOnCircle(center, GeometryRadius, 0d);
        var midPoint = PointOnCircle(center, GeometryRadius, 180d);
        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(startPoint, isFilled: false, isClosed: false);
            ctx.ArcTo(
                midPoint,
                new Size(GeometryRadius, GeometryRadius),
                rotationAngle: 0,
                isLargeArc: false,
                sweepDirection: SweepDirection.Clockwise,
                isStroked: true,
                isSmoothJoin: false);

            ctx.ArcTo(
                startPoint,
                new Size(GeometryRadius, GeometryRadius),
                rotationAngle: 0,
                isLargeArc: false,
                sweepDirection: SweepDirection.Clockwise,
                isStroked: true,
                isSmoothJoin: false);
        }

        geometry.Freeze();
        return geometry;
    }
    private static readonly Geometry FullCircleGeometry = CreateFullCircleGeometry();

    /// <summary>
    /// Calculates a point on the circumference of a circle given its center, radius, and angle in degrees.
    /// </summary>
    /// <param name="center">Center point of the circle.</param>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="angleDegrees">Angle in degrees.</param>
    /// <returns>Point on the circle at the specified angle.</returns>
    private static Point PointOnCircle(Point center, double radius, double angleDegrees)
    {
        var angleRadians = angleDegrees * Math.PI / 180d;
        var x = center.X + radius * Math.Cos(angleRadians);
        var y = center.Y + radius * Math.Sin(angleRadians);
        return new Point(x, y);
    }
    #endregion
}
