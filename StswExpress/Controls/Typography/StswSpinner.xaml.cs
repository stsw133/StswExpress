using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace StswExpress;
/// <summary>
/// Represents a control representing that the app is loading content or performing another process that the user needs to wait on.
/// </summary>
public class StswSpinner : Control
{
    static StswSpinner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSpinner), new FrameworkPropertyMetadata(typeof(StswSpinner)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswSpinner), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the scale of the loading circle.
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
            typeof(StswSpinner),
            new FrameworkPropertyMetadata(default(GridLength),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswSpinner stsw)
        {
            stsw.Height = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
            stsw.Width = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
        }
    }

    /// <summary>
    /// Gets or sets type of spinner icon.
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
            typeof(StswSpinner)
        );
    #endregion

    #region Style properties
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
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:
    /*
    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? Background { get; private set; }
    */
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
}

/// <summary>
/// 
/// </summary>
internal class StswBusyAnimation2 : Control
{
    static StswBusyAnimation2()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswBusyAnimation2), new FrameworkPropertyMetadata(typeof(StswBusyAnimation2)));
    }

    private Ellipse? _ellipseMain;
    private Ellipse?[] _ellipses = new Ellipse[15];

    /// <summary>
    /// 
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _ellipseMain = GetTemplateChild("PART_EllipseMain") as Ellipse;
        Enumerable.Range(1, _ellipses.Length - 1).ForEach(x => _ellipses[x] = GetTemplateChild("PART_Ellipse" + x) as Ellipse);

        if (Active)
            InitAnimation();
    }

    /// <summary>
    /// 
    /// </summary>
    public bool Active
    {
        get => (bool)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }
    public static readonly DependencyProperty ActiveProperty
        = DependencyProperty.Register(
            nameof(Active),
            typeof(bool),
            typeof(StswBusyAnimation2),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.None, OnActiveChanged)
        );
    private static void OnActiveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswBusyAnimation2 stsw)
        {
            if (!stsw.IsLoaded)
                return;

            if (stsw.Active)
                stsw.InitAnimation();
            else
                stsw.EndAnimation();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void InitAnimation()
    {
        var sb = new Storyboard();

        var mainEllipseWidthAnim = new DoubleAnimation(
            fromValue: 300,
            toValue: 0,
            duration: TimeSpan.FromMilliseconds(500),
            fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(mainEllipseWidthAnim);
        Storyboard.SetTarget(mainEllipseWidthAnim, _ellipseMain);
        Storyboard.SetTargetProperty(mainEllipseWidthAnim, new PropertyPath(WidthProperty));

        var mainEllipseHeightAnim = new DoubleAnimation(
           fromValue: 300,
           toValue: 0,
           duration: TimeSpan.FromMilliseconds(500),
           fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(mainEllipseHeightAnim);
        Storyboard.SetTarget(mainEllipseHeightAnim, _ellipseMain);
        Storyboard.SetTargetProperty(mainEllipseHeightAnim, new PropertyPath(HeightProperty));

        var mainEllipseMarginAnim = new ThicknessAnimation(
           fromValue: new Thickness(0),
           toValue: new Thickness(150),
           duration: TimeSpan.FromMilliseconds(500),
           fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(mainEllipseMarginAnim);
        Storyboard.SetTarget(mainEllipseMarginAnim, _ellipseMain);
        Storyboard.SetTargetProperty(mainEllipseMarginAnim, new PropertyPath(MarginProperty));

        Enumerable.Range(1, _ellipses.Length - 1).ForEach(x => GoToStartPositionAnimation(_ellipses[x], (x - 1) / 2 * 125, sb));

        sb.Completed += (s, e) => Enumerable.Range(1, _ellipses.Length - 1).ForEach(x => StartOrbitingAnimation(_ellipses[x]!, 285 * ((x - 1) / 2) + (((x - 1) / 2) - 1), x % 2 == 0));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ellipse"></param>
    /// <param name="timeOffset"></param>
    /// <param name="fromBack"></param>
    private void StartOrbitingAnimation(Ellipse ellipse, int timeOffset, bool fromBack)
    {
        var sb = new Storyboard()
        {
            BeginTime = TimeSpan.FromMilliseconds(timeOffset)
        };

        if (!fromBack)
        {
            var orbitEllipseAnim = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteDoubleKeyFrame(375, KeyTime.FromPercent(0)),
                    new EasingDoubleKeyFrame(575, KeyTime.FromPercent(.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.7 }),
                    new EasingDoubleKeyFrame(375, KeyTime.FromPercent(.5), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.7 }),
                    new EasingDoubleKeyFrame(175, KeyTime.FromPercent(.75), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.7 }),
                    new EasingDoubleKeyFrame(375, KeyTime.FromPercent(1), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.7 })
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(orbitEllipseAnim);
            Storyboard.SetTarget(orbitEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(orbitEllipseAnim, new PropertyPath(Canvas.TopProperty));

            var opacityEllipseAnim = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(0)),
                    new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(.25)),
                    new EasingDoubleKeyFrame(0, KeyTime.FromPercent(.45), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.7 }),
                    new DiscreteDoubleKeyFrame(0, KeyTime.FromPercent(.55)),
                    new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0.75), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.7 }),
                    new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(1)),
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(opacityEllipseAnim);
            Storyboard.SetTarget(opacityEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(opacityEllipseAnim, new PropertyPath(OpacityProperty));

            var widthEllipseAnim = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteDoubleKeyFrame(100, KeyTime.FromPercent(0)),
                    new EasingDoubleKeyFrame(70, KeyTime.FromPercent(.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                    new EasingDoubleKeyFrame(20, KeyTime.FromPercent(.5), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingDoubleKeyFrame(70, KeyTime.FromPercent(.75), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingDoubleKeyFrame(100, KeyTime.FromPercent(1), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(widthEllipseAnim);
            Storyboard.SetTarget(widthEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(widthEllipseAnim, new PropertyPath(WidthProperty));

            var heightEllipseAnim = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteDoubleKeyFrame(100, KeyTime.FromPercent(0)),
                    new EasingDoubleKeyFrame(70, KeyTime.FromPercent(.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                    new EasingDoubleKeyFrame(20, KeyTime.FromPercent(.5), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingDoubleKeyFrame(70, KeyTime.FromPercent(.75), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingDoubleKeyFrame(100, KeyTime.FromPercent(1), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(heightEllipseAnim);
            Storyboard.SetTarget(heightEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(heightEllipseAnim, new PropertyPath(HeightProperty));

            var marginEllipseAnim = new ThicknessAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteThicknessKeyFrame(new Thickness(0), KeyTime.FromPercent(0)),
                    new EasingThicknessKeyFrame(new Thickness(15), KeyTime.FromPercent(.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                    new EasingThicknessKeyFrame(new Thickness(40), KeyTime.FromPercent(.5), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingThicknessKeyFrame(new Thickness(15), KeyTime.FromPercent(.75), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingThicknessKeyFrame(new Thickness(0), KeyTime.FromPercent(1), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(marginEllipseAnim);
            Storyboard.SetTarget(marginEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(marginEllipseAnim, new PropertyPath(MarginProperty));
        }
        else
        {
            var orbitEllipseAnim = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteDoubleKeyFrame(375, KeyTime.FromPercent(0)),
                    new EasingDoubleKeyFrame(175, KeyTime.FromPercent(.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.7 }),
                    new EasingDoubleKeyFrame(375, KeyTime.FromPercent(.5), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.7 }),
                    new EasingDoubleKeyFrame(575, KeyTime.FromPercent(.75), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.7 }),
                    new EasingDoubleKeyFrame(375, KeyTime.FromPercent(1), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.7 })
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(orbitEllipseAnim);
            Storyboard.SetTarget(orbitEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(orbitEllipseAnim, new PropertyPath(Canvas.TopProperty));

            var opacityEllipseAnim = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteDoubleKeyFrame(0, KeyTime.FromPercent(0)),
                    new DiscreteDoubleKeyFrame(0, KeyTime.FromPercent(.05)),
                    new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.7 }),
                    new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(0.75)),
                    new EasingDoubleKeyFrame(0, KeyTime.FromPercent(.95), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.7 }),
                    new DiscreteDoubleKeyFrame(0, KeyTime.FromPercent(1)),
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(opacityEllipseAnim);
            Storyboard.SetTarget(opacityEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(opacityEllipseAnim, new PropertyPath(OpacityProperty));

            var widthEllipseAnim = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteDoubleKeyFrame(20, KeyTime.FromPercent(0)),
                    new EasingDoubleKeyFrame(70, KeyTime.FromPercent(.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                    new EasingDoubleKeyFrame(100, KeyTime.FromPercent(.5), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingDoubleKeyFrame(70, KeyTime.FromPercent(.75), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingDoubleKeyFrame(20, KeyTime.FromPercent(1), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(widthEllipseAnim);
            Storyboard.SetTarget(widthEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(widthEllipseAnim, new PropertyPath(WidthProperty));

            var heightEllipseAnim = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteDoubleKeyFrame(20, KeyTime.FromPercent(0)),
                    new EasingDoubleKeyFrame(70, KeyTime.FromPercent(.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                    new EasingDoubleKeyFrame(100, KeyTime.FromPercent(.5), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingDoubleKeyFrame(70, KeyTime.FromPercent(.75), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingDoubleKeyFrame(20, KeyTime.FromPercent(1), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(heightEllipseAnim);
            Storyboard.SetTarget(heightEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(heightEllipseAnim, new PropertyPath(HeightProperty));

            var marginEllipseAnim = new ThicknessAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(4000),
                KeyFrames =
                [
                    new DiscreteThicknessKeyFrame(new Thickness(40), KeyTime.FromPercent(0)),
                    new EasingThicknessKeyFrame(new Thickness(15), KeyTime.FromPercent(.25), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                    new EasingThicknessKeyFrame(new Thickness(0), KeyTime.FromPercent(.5), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingThicknessKeyFrame(new Thickness(15), KeyTime.FromPercent(.75), new PowerEase(){ EasingMode = EasingMode.EaseIn, Power=1.5 }),
                    new EasingThicknessKeyFrame(new Thickness(40), KeyTime.FromPercent(1), new PowerEase(){ EasingMode = EasingMode.EaseOut, Power=1.5 }),
                ],
                RepeatBehavior = RepeatBehavior.Forever,
                FillBehavior = FillBehavior.HoldEnd
            };
            sb.Children.Add(marginEllipseAnim);
            Storyboard.SetTarget(marginEllipseAnim, ellipse);
            Storyboard.SetTargetProperty(marginEllipseAnim, new PropertyPath(MarginProperty));
        }

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ellipse"></param>
    /// <param name="left"></param>
    /// <param name="sb"></param>
    private void GoToStartPositionAnimation(Ellipse? ellipse, int left, Storyboard sb)
    {
        var leftEllipseAnim = new DoubleAnimation(
            toValue: left,
            duration: TimeSpan.FromMilliseconds(500),
            fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(leftEllipseAnim);
        Storyboard.SetTarget(leftEllipseAnim, ellipse);
        Storyboard.SetTargetProperty(leftEllipseAnim, new PropertyPath(Canvas.LeftProperty));
    }

    /// <summary>
    /// 
    /// </summary>
    private void EndAnimation()
    {
        var sb = new Storyboard();

        Enumerable.Range(1, _ellipses.Length - 1).ForEach(x => AddEllipseSlideInAnimation(_ellipses[x]!, sb));

        var mainEllipseWidthAnim = new DoubleAnimation(
            toValue: 300,
            duration: TimeSpan.FromMilliseconds(500),
            fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(mainEllipseWidthAnim);
        Storyboard.SetTarget(mainEllipseWidthAnim, _ellipseMain);
        Storyboard.SetTargetProperty(mainEllipseWidthAnim, new PropertyPath(WidthProperty));

        var mainEllipseHeightAnim = new DoubleAnimation(
            toValue: 300,
            duration: TimeSpan.FromMilliseconds(500),
            fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(mainEllipseHeightAnim);
        Storyboard.SetTarget(mainEllipseHeightAnim, _ellipseMain);
        Storyboard.SetTargetProperty(mainEllipseHeightAnim, new PropertyPath(HeightProperty));

        var mainEllipseMarginAnim = new ThicknessAnimation(
            toValue: new Thickness(0),
            duration: TimeSpan.FromMilliseconds(500),
            fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(mainEllipseMarginAnim);
        Storyboard.SetTarget(mainEllipseMarginAnim, _ellipseMain);
        Storyboard.SetTargetProperty(mainEllipseMarginAnim, new PropertyPath(MarginProperty));

        sb.Completed += (s, e) => Enumerable.Range(1, _ellipses.Length - 1).ForEach(x => RemoveEllipseAnimation(_ellipses[x]!));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ellipse"></param>
    private void RemoveEllipseAnimation(Ellipse ellipse)
    {
        ellipse.BeginAnimation(WidthProperty, null);
        ellipse.BeginAnimation(HeightProperty, null);
        ellipse.BeginAnimation(MarginProperty, null);
        ellipse.BeginAnimation(OpacityProperty, null);

        ellipse.Width = 100;
        ellipse.Height = 100;
        ellipse.Margin = new Thickness(0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ellipse"></param>
    /// <param name="sb"></param>
    private void AddEllipseSlideInAnimation(Ellipse ellipse, Storyboard sb)
    {
        var ellipseLeftAnim = new DoubleAnimation(
            toValue: 375,
            duration: TimeSpan.FromMilliseconds(500),
            fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(ellipseLeftAnim);
        Storyboard.SetTarget(ellipseLeftAnim, ellipse);
        Storyboard.SetTargetProperty(ellipseLeftAnim, new PropertyPath(Canvas.LeftProperty));

        var ellipseTopAnim = new DoubleAnimation(
            toValue: 375,
            duration: TimeSpan.FromMilliseconds(500),
            fillBehavior: FillBehavior.HoldEnd)
        {
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(ellipseTopAnim);
        Storyboard.SetTarget(ellipseTopAnim, ellipse);
        Storyboard.SetTargetProperty(ellipseTopAnim, new PropertyPath(Canvas.TopProperty));
    }
}
