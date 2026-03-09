using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress.Wpf;
/// <summary>
/// A loading spinner control for indicating ongoing background processes.
/// Supports different animation styles, scaling, and color customization.
/// </summary>
/// <remarks>
/// The control provides visual feedback for loading states, making it useful for asynchronous operations.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSpinner Type="Dots" Scale="2" Fill="Red"/&gt;
/// </code>
/// </example>
public class StswSpinner : FrameworkElement
{
    static StswSpinner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSpinner), new FrameworkPropertyMetadata(typeof(StswSpinner)));
    }
    public StswSpinner()
    {
        Loaded += (_, _) => UpdateAnimationState();
        Unloaded += (_, _) => UpdateAnimationState();
        IsVisibleChanged += (_, _) => UpdateAnimationState();
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets the fill brush of the loading circle.
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
            typeof(StswSpinner),
            new FrameworkPropertyMetadata(default(Brush),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the foreground brush of the spinner.
    /// This value is inherited from parent controls and used as a fallback for <see cref="Fill"/>.
    /// </summary>
    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
    public static readonly DependencyProperty ForegroundProperty
        = Control.ForegroundProperty.AddOwner(
            typeof(StswSpinner),
            new FrameworkPropertyMetadata(SystemColors.ControlTextBrush,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the scale of the loading circle.
    /// Determines the overall size of the spinner.
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
            typeof(StswSpinner),
            new FrameworkPropertyMetadata(default(GridLength?),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswSpinner)d;
        IStswIconControl.ScaleChanged(stsw, stsw.Scale);
    }

    /// <summary>
    /// Gets or sets the type of spinner animation.
    /// Allows selecting different visual styles.
    /// </summary>
    public StswSpinnerType Type
    {
        get => (StswSpinnerType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
    public static readonly DependencyProperty TypeProperty
        = DependencyProperty.Register(
            nameof(Type),
            typeof(StswSpinnerType),
            typeof(StswSpinner),
            new FrameworkPropertyMetadata(StswSpinnerType.Circles,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnTypeChanged)
        );
    private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswSpinner)d;
        stsw._penCache = null;
        stsw.InvalidateVisual();
    }
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
        var scale = GetScaleFactor();
        var desiredWidth = double.IsNaN(Width) ? 24 * scale : Width;
        var desiredHeight = double.IsNaN(Height) ? 24 * scale : Height;

        if (!double.IsInfinity(availableSize.Width))
            desiredWidth = Math.Min(availableSize.Width, desiredWidth);

        if (!double.IsInfinity(availableSize.Height))
            desiredHeight = Math.Min(availableSize.Height, desiredHeight);

        return new(desiredWidth, desiredHeight);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IsEnabledProperty)
        {
            UpdateAnimationState();
            InvalidateVisual();
        }
    }

    /// <inheritdoc/>
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var size = Math.Min(RenderSize.Width, RenderSize.Height);
        if (size <= 0)
            return;

        var fill = Fill ?? Foreground ?? Brushes.Gray;
        var center = new Point(RenderSize.Width / 2, RenderSize.Height / 2);

        switch (Type)
        {
            case StswSpinnerType.Bars:
                DrawBars(drawingContext, center, size, fill);
                break;
            case StswSpinnerType.Circles:
                DrawCircles(drawingContext, center, size, fill);
                break;
            case StswSpinnerType.Crescent:
                DrawCrescent(drawingContext, center, size, fill);
                break;
            case StswSpinnerType.Dots:
                DrawDots(drawingContext, center, size, fill);
                break;
            case StswSpinnerType.Helix:
                DrawHelix(drawingContext, center, size, fill);
                break;
            case StswSpinnerType.Lines:
                DrawLines(drawingContext, center, size, fill);
                break;
            case StswSpinnerType.Pulse:
                DrawPulse(drawingContext, center, size, fill);
                break;
        }
    }
    #endregion

    #region Logic
    private bool _isAnimating;
    private double _nowSeconds;
    private Pen? _penCache;

    /// <summary>
    /// Animation frame update handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void OnRendering(object? sender, EventArgs e)
    {
        if (!_isAnimating)
            return;

        if (e is RenderingEventArgs rea)
        {
            _nowSeconds = rea.RenderingTime.TotalSeconds;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Evaluates a value within a segment using easing.
    /// </summary>
    /// <param name="t">Current time value.</param>
    /// <param name="t0">Start time of the segment.</param>
    /// <param name="t1">End time of the segment.</param>
    /// <param name="v0">Start value of the segment.</param>
    /// <param name="v1">End value of the segment.</param>
    /// <param name="easeOut">Whether to use ease-out easing.</param>
    /// <param name="power">Easing power.</param>
    /// <returns>Evaluated value.</returns>
    private static double EvalSegment(double t, double t0, double t1, double v0, double v1, bool easeOut, double power)
    {
        if (t <= t0) return v0;
        if (t >= t1) return v1;

        double u = (t - t0) / (t1 - t0);
        u = easeOut ? EaseOutPow(u, power) : EaseInPow(u, power);
        return StswMath.Lerp(v0, v1, u);
    }

    /// <summary>
    /// Calculates the progress of the current animation cycle.
    /// </summary>
    /// <param name="durationSeconds">Duration of one animation cycle in seconds.</param>
    /// <returns>Progress value between 0 and 1.</returns>
    private double GetProgress(double durationSeconds)
    {
        if (!_isAnimating || durationSeconds <= 0)
            return 0;

        return (_nowSeconds % durationSeconds) / durationSeconds;
    }

    /// <summary>
    /// Gets the scale factor based on the Scale property.
    /// </summary>
    /// <returns>Scale factor as a double.</returns>
    private double GetScaleFactor() => Scale.HasValue && !Scale.Value.IsStar ? Scale.Value.Value : 1;

    /// <summary>
    /// Updates animation subscription depending on control visibility and global settings.
    /// </summary>
    private void UpdateAnimationState()
    {
        var shouldAnimate =
            StswApp.Settings.AnimationsEnabled &&
            StswControl.GetEnableAnimations(this) &&
            IsLoaded &&
            IsVisible &&
            IsEnabled;

        if (shouldAnimate && !_isAnimating)
        {
            CompositionTarget.Rendering += OnRendering;
            _isAnimating = true;
        }
        else if (!shouldAnimate && _isAnimating)
        {
            CompositionTarget.Rendering -= OnRendering;
            _isAnimating = false;
        }
    }
    #endregion

    #region Helpers
    private const double TwoPi = Math.PI * 2;

    /// <summary>
    /// Gets a cached pen with specified brush and thickness.
    /// </summary>
    /// <param name="brush">Brush of the pen.</param>
    /// <param name="thickness">Thickness of the pen.</param>
    /// <returns>Cached pen instance.</returns>
    private Pen GetPen(Brush brush, double thickness)
    {
        _penCache ??= new Pen(brush, thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };

        _penCache.Brush = brush;
        _penCache.Thickness = thickness;
        return _penCache;
    }

    private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
    private static double EaseInPow(double t, double power) { t = Clamp01(t); return Math.Pow(t, power); }
    private static double EaseInPower2(double v) { v = Clamp01(v); return v * v; }
    private static double EaseOutPow(double t, double power) { t = Clamp01(t); return 1.0 - Math.Pow(1.0 - t, power); }
    private static double EaseOutPower2(double v) => 1 - EaseInPower2(1 - Clamp01(v));
    private static Point PointOnCircle(Point c, double r, double deg)
    {
        var a = deg * Math.PI / 180;
        return new(c.X + r * Math.Cos(a), c.Y + r * Math.Sin(a));
    }
    #endregion

    #region Animations
    /// <summary>
    /// Draws bouncing bars similar to an equalizer.
    /// </summary>
    /// <param name="dc">Drawing context.</param>
    /// <param name="center">Center point of the spinner.</param>
    /// <param name="size">Overall size of the spinner.</param>
    /// <param name="brush">Brush used for drawing.</param>
    private void DrawBars(DrawingContext dc, Point center, double size, Brush brush)
    {
        const int bars = 5;
        var progress = GetProgress(1.2);
        var barWidth = size * 0.12;
        var spacing = size * 0.08;
        var maxHeight = size * 0.7;
        var startX = center.X - ((bars - 1) * (barWidth + spacing)) / 2;

        for (var i = 0; i < bars; i++)
        {
            var offset = i / (double)bars;
            var phase = (progress - offset + 1.0) % 1.0;

            var wave = 0.5 - 0.5 * Math.Cos(TwoPi * phase);
            var height = size * (0.18 + 0.52 * wave);
            var top = center.Y + maxHeight / 2 - height;

            var rect = new Rect(
                startX + i * (barWidth + spacing),
                top,
                barWidth,
                height);

            var barOpacity = 0.35 + 0.65 * wave;

            dc.PushOpacity(barOpacity);
            dc.DrawRoundedRectangle(brush, null, rect, barWidth / 2, barWidth / 2);
            dc.Pop();
        }
    }

    /// <summary>
    /// Draws a series of circles arranged in a circular pattern.
    /// </summary>
    /// <param name="dc">Drawing context.</param>
    /// <param name="center">Center point of the spinner.</param>
    /// <param name="size">Overall size of the spinner.</param>
    /// <param name="brush">Brush used for drawing.</param>
    private void DrawCircles(DrawingContext dc, Point center, double size, Brush brush)
    {
        const int dots = 8;
        var ringRadius = (size / 2) - size * 0.12;
        var dotRadius = size * 0.1;
        var progress = GetProgress(1);

        for (var i = 0; i < dots; i++)
        {
            var offset = i / (double)dots;
            var phase = (progress - offset + 1) % 1.0;

            var opacity = phase <= 0.9 ? 1.0 - EaseInPower2(phase / 0.9) : 0.0;
            if (opacity <= 0)
                continue;

            var angle = offset * TwoPi;
            var dotCenter = new Point(
                center.X + ringRadius * Math.Cos(angle),
                center.Y + ringRadius * Math.Sin(angle));

            dc.PushOpacity(opacity);
            dc.DrawEllipse(brush, null, dotCenter, dotRadius, dotRadius);
            dc.Pop();
        }
    }

    /// <summary>
    /// Draws a crescent-shaped spinner animation.
    /// </summary>
    /// <param name="dc">Drawing context.</param>
    /// <param name="center">Center point of the spinner.</param>
    /// <param name="size">Overall size of the spinner.</param>
    /// <param name="brush">Brush used for drawing.</param>
    private void DrawCrescent(DrawingContext dc, Point center, double size, Brush brush)
    {
        var ringRadius = size / 2 - size * 0.1;
        var progress = GetProgress(1.2);
        var startAngle = progress * 360;
        const double sweep = 240;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            var start = PointOnCircle(center, ringRadius, startAngle);
            var end = PointOnCircle(center, ringRadius, startAngle + sweep);
            ctx.BeginFigure(start, false, false);
            ctx.ArcTo(end, new Size(ringRadius, ringRadius), 0, sweep > 180, SweepDirection.Clockwise, true, false);
        }

        geometry.Freeze();
        var pen = new Pen(brush, size * 0.12)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };

        dc.DrawGeometry(null, pen, geometry);
    }

    /// <summary>
    /// Draws a series of dots that animate in a linear sequence.
    /// </summary>
    /// <param name="dc">Drawing context.</param>
    /// <param name="center">Center point of the spinner.</param>
    /// <param name="size">Overall size of the spinner.</param>
    /// <param name="brush">Brush used for drawing.</param>
    private void DrawDots(DrawingContext dc, Point center, double size, Brush brush)
    {
        const int dots = 3;
        var cycleSeconds = 0.9;
        var progress = GetProgress(cycleSeconds);

        var dotSpacing = size * 0.35;
        var baseDotRadius = size * 0.12;
        var timeShiftPerDot = 0.2 / cycleSeconds;

        for (var i = 0; i < dots; i++)
        {
            var offset = i * timeShiftPerDot;
            var phase = (progress - offset + 1) % 1;

            var t1 = 0.4 / cycleSeconds;
            var t2 = 0.8 / cycleSeconds;

            double opacity;
            double scale;

            if (phase <= t1)
            {
                var t = phase / t1;
                t = EaseInPower2(t);

                opacity = StswMath.Lerp(0.3, 1.0, t);
                scale = StswMath.Lerp(0.25, 1.0, t);
            }
            else if (phase <= t2)
            {
                var t = (phase - t1) / (t2 - t1);
                t = EaseOutPower2(t);

                opacity = StswMath.Lerp(1.0, 0.3, t);
                scale = StswMath.Lerp(1.0, 0.25, t);
            }
            else
            {
                opacity = 0.3;
                scale = 0.25;
            }

            var dotCenter = new Point(
                center.X + (i - 1) * dotSpacing,
                center.Y);

            dc.PushOpacity(opacity);
            dc.DrawEllipse(brush, null, dotCenter, baseDotRadius * scale, baseDotRadius * scale);
            dc.Pop();
        }
    }

    /// <summary>
    /// Draws a helix-style spinner animation.
    /// </summary>
    /// <param name="dc">Drawing context.</param>
    /// <param name="center">Center point of the spinner.</param>
    /// <param name="size">Overall size of the spinner.</param>
    /// <param name="brush">Brush used for drawing.</param>
    private void DrawHelix(DrawingContext dc, Point center, double size, Brush brush)
    {
        const double baseCanvasSize = 850.0;
        const double orbitSeconds = 4.0;
        const double sizeScale = 1.2;

        var scaleToControl = size / baseCanvasSize;
        var offsetX = center.X - baseCanvasSize * scaleToControl / 2.0;
        var offsetY = center.Y - baseCanvasSize * scaleToControl / 2.0;

        var orbitPhase = GetProgress(orbitSeconds);

        for (var i = 1; i <= 14; i++)
        {
            var group = (i - 1) / 2;
            var fromBack = (i % 2) == 0;

            var timeOffsetSeconds = (285.0 * group + (group - 1)) / 1000.0;
            var phase = (orbitPhase + (timeOffsetSeconds / orbitSeconds)) % 1.0;
            var left = group * 125.0;

            double top;
            if (!fromBack)
            {
                if (phase <= 0.25) top = EvalSegment(phase, 0.00, 0.25, 375, 575, easeOut: true, power: 1.7);
                else if (phase <= 0.50) top = EvalSegment(phase, 0.25, 0.50, 575, 375, easeOut: false, power: 1.7);
                else if (phase <= 0.75) top = EvalSegment(phase, 0.50, 0.75, 375, 175, easeOut: true, power: 1.7);
                else top = EvalSegment(phase, 0.75, 1.00, 175, 375, easeOut: false, power: 1.7);
            }
            else
            {
                if (phase <= 0.25) top = EvalSegment(phase, 0.00, 0.25, 375, 175, easeOut: true, power: 1.7);
                else if (phase <= 0.50) top = EvalSegment(phase, 0.25, 0.50, 175, 375, easeOut: false, power: 1.7);
                else if (phase <= 0.75) top = EvalSegment(phase, 0.50, 0.75, 375, 575, easeOut: true, power: 1.7);
                else top = EvalSegment(phase, 0.75, 1.00, 575, 375, easeOut: false, power: 1.7);
            }

            double opacity;
            if (!fromBack)
            {
                if (phase < 0.25) opacity = 1;
                else if (phase < 0.45) opacity = StswMath.Lerp(1, 0, EaseInPow((phase - 0.25) / (0.45 - 0.25), 1.7));
                else if (phase < 0.55) opacity = 0;
                else if (phase < 0.75) opacity = StswMath.Lerp(0, 1, EaseOutPow((phase - 0.55) / (0.75 - 0.55), 1.7));
                else opacity = 1;
            }
            else
            {
                if (phase < 0.05) opacity = 0;
                else if (phase < 0.25) opacity = StswMath.Lerp(0, 1, EaseOutPow((phase - 0.05) / (0.25 - 0.05), 1.7));
                else if (phase < 0.75) opacity = 1;
                else if (phase < 0.95) opacity = StswMath.Lerp(1, 0, EaseInPow((phase - 0.75) / (0.95 - 0.75), 1.7));
                else opacity = 0;
            }

            if (opacity <= 0.001)
                continue;

            double ellipseSize;
            double margin;

            if (!fromBack)
            {
                if (phase <= 0.25) ellipseSize = EvalSegment(phase, 0.00, 0.25, 100, 70, easeOut: true, power: 1.5);
                else if (phase <= 0.50) ellipseSize = EvalSegment(phase, 0.25, 0.50, 70, 20, easeOut: false, power: 1.5);
                else if (phase <= 0.75) ellipseSize = EvalSegment(phase, 0.50, 0.75, 20, 70, easeOut: false, power: 1.5);
                else ellipseSize = EvalSegment(phase, 0.75, 1.00, 70, 100, easeOut: true, power: 1.5);

                if (phase <= 0.25) margin = EvalSegment(phase, 0.00, 0.25, 0, 15, easeOut: true, power: 1.5);
                else if (phase <= 0.50) margin = EvalSegment(phase, 0.25, 0.50, 15, 40, easeOut: false, power: 1.5);
                else if (phase <= 0.75) margin = EvalSegment(phase, 0.50, 0.75, 40, 15, easeOut: false, power: 1.5);
                else margin = EvalSegment(phase, 0.75, 1.00, 15, 0, easeOut: true, power: 1.5);
            }
            else
            {
                if (phase <= 0.25) ellipseSize = EvalSegment(phase, 0.00, 0.25, 20, 70, easeOut: true, power: 1.5);
                else if (phase <= 0.50) ellipseSize = EvalSegment(phase, 0.25, 0.50, 70, 100, easeOut: false, power: 1.5);
                else if (phase <= 0.75) ellipseSize = EvalSegment(phase, 0.50, 0.75, 100, 70, easeOut: false, power: 1.5);
                else ellipseSize = EvalSegment(phase, 0.75, 1.00, 70, 20, easeOut: true, power: 1.5);

                if (phase <= 0.25) margin = EvalSegment(phase, 0.00, 0.25, 40, 15, easeOut: true, power: 1.5);
                else if (phase <= 0.50) margin = EvalSegment(phase, 0.25, 0.50, 15, 0, easeOut: false, power: 1.5);
                else if (phase <= 0.75) margin = EvalSegment(phase, 0.50, 0.75, 0, 15, easeOut: false, power: 1.5);
                else margin = EvalSegment(phase, 0.75, 1.00, 15, 40, easeOut: true, power: 1.5);
            }

            ellipseSize *= sizeScale;
            double drawLeft = left + margin;
            double drawTop = top + margin;
            double drawSize = Math.Max(0.1, ellipseSize - 2 * margin);

            var ellipseRect = new Rect(
                offsetX + drawLeft * scaleToControl,
                offsetY + drawTop * scaleToControl,
                drawSize * scaleToControl,
                drawSize * scaleToControl);

            dc.PushOpacity(opacity);
            dc.DrawEllipse(brush, null,
                new Point(ellipseRect.X + ellipseRect.Width / 2, ellipseRect.Y + ellipseRect.Height / 2),
                ellipseRect.Width / 2, ellipseRect.Height / 2);
            dc.Pop();
        }
    }

    /// <summary>
    /// Draws a series of lines arranged in a circular pattern.
    /// </summary>
    /// <param name="dc">Drawing context.</param>
    /// <param name="center">Center point of the spinner.</param>
    /// <param name="size">Overall size of the spinner.</param>
    /// <param name="brush">Brush used for drawing.</param>
    private void DrawLines(DrawingContext dc, Point center, double size, Brush brush)
    {
        const int lines = 8;
        var outerRadius = (size / 2) - size * 0.12;
        var innerRadius = outerRadius - size * 0.18;
        var progress = GetProgress(1);
        var pen = GetPen(brush, size * 0.08);

        for (var i = 0; i < lines; i++)
        {
            var phase = (progress - i / (double)lines + 1) % 1;
            var opacity = phase <= 0.9 ? 1.0 - EaseInPower2(phase / 0.9) : 0.0;
            if (opacity <= 0)
                continue;

            var angle = i * TwoPi / lines;
            var outer = new Point(
                center.X + outerRadius * Math.Cos(angle),
                center.Y + outerRadius * Math.Sin(angle));
            var inner = new Point(
                center.X + innerRadius * Math.Cos(angle),
                center.Y + innerRadius * Math.Sin(angle));

            dc.PushOpacity(opacity);
            dc.DrawLine(pen, inner, outer);
            dc.Pop();
        }
    }

    /// <summary>
    /// Draws a pulsing ring animation.
    /// </summary>
    /// <param name="dc">Drawing context.</param>
    /// <param name="center">Center point of the spinner.</param>
    /// <param name="size">Overall size of the spinner.</param>
    /// <param name="brush">Brush used for drawing.</param>
    private void DrawPulse(DrawingContext dc, Point center, double size, Brush brush)
    {
        var progress = GetProgress(1.5);
        var pen = GetPen(brush, size * 0.08);
        var centerDotRadius = size * 0.13;

        DrawPulseRing(dc, center, size, pen, progress);
        DrawPulseRing(dc, center, size, pen, (progress + 0.5) % 1);
        dc.DrawEllipse(brush, null, center, centerDotRadius, centerDotRadius);
    }

    /// <summary>
    /// Draws a single pulse ring at a given progress state.
    /// </summary>
    /// <param name="dc">Drawing context.</param>
    /// <param name="center">Center point of the spinner.</param>
    /// <param name="size">Overall size of the spinner.</param>
    /// <param name="pen">Pen used for drawing the ring.</param>
    /// <param name="progress">Progress of the pulse animation (0 to 1).</param>
    private static void DrawPulseRing(DrawingContext dc, Point center, double size, Pen pen, double progress)
    {
        const double rMin = 0.18;
        const double rMax = 0.52;
        var ringRadius = size * (rMin + (rMax - rMin) * progress);
        const double a = 0.25;
        const double b = 0.75;

        double w;
        if (progress < a)
        {
            var x = progress / a;
            w = x * x * (3 - 2 * x);
        }
        else if (progress > b)
        {
            var x = (1 - progress) / (1 - b);
            w = x * x * (3 - 2 * x);
        }
        else
        {
            w = 1.0;
        }

        var opacity = 0.65 * w;
        if (opacity <= 0.001)
            return;

        dc.PushOpacity(opacity);
        dc.DrawEllipse(null, pen, center, ringRadius, ringRadius);
        dc.Pop();
    }
    #endregion
}
