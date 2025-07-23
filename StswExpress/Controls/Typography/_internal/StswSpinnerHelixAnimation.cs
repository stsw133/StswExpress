using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace StswExpress;
/// <summary>
/// Represents a secondary busy animation with dynamic movement.
/// Used as part of the <see cref="StswSpinner"/> control.
/// </summary>
[StswInfo("0.15.0")]
internal class StswSpinnerHelixAnimation : Control
{
    private readonly Ellipse?[] _ellipses = new Ellipse[15];
    private Ellipse? _ellipseMain;

    static StswSpinnerHelixAnimation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSpinnerHelixAnimation), new FrameworkPropertyMetadata(typeof(StswSpinnerHelixAnimation)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _ellipseMain = GetTemplateChild("PART_EllipseMain") as Ellipse;
        Enumerable.Range(1, _ellipses.Length - 1).ForEach(x => _ellipses[x] = GetTemplateChild($"PART_Ellipse{x}") as Ellipse);

        if (IsActive)
            InitAnimation();
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the animation is active.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
    public static readonly DependencyProperty IsActiveProperty
        = DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(StswSpinnerHelixAnimation),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.None, OnIsActiveChanged)
        );
    private static void OnIsActiveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswSpinnerHelixAnimation stsw)
            return;

        if (!stsw.IsLoaded)
            return;

        stsw.IsActive.Do(stsw.InitAnimation, stsw.EndAnimation);
    }
    #endregion

    #region Animation
    /// <summary>
    /// Starts the animation sequence when the control becomes active.
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
    /// Ends the animation sequence when the control is deactivated.
    /// </summary>
    private void EndAnimation()
    {
        var sb = new Storyboard();

        Enumerable.Range(1, _ellipses.Length - 1).ForEach(x => AddEllipseSlideInAnimation(_ellipses[x]!, sb));

        var mainEllipseWidthAnim = new DoubleAnimation(
            fromValue: _ellipseMain.ActualWidth != double.NaN ? _ellipseMain.ActualWidth : 0.0,
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
            fromValue: _ellipseMain.ActualHeight != double.NaN ? _ellipseMain.ActualHeight : 0.0,
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
    /// Adds an animation effect that moves an ellipse inward, simulating a "slide-in" effect.
    /// </summary>
    /// <param name="ellipse">The ellipse to animate.</param>
    /// <param name="sb">The storyboard managing the animation.</param>
    private static void AddEllipseSlideInAnimation(Ellipse ellipse, Storyboard sb)
    {
        var ellipseLeftAnim = new DoubleAnimation(
            fromValue: Canvas.GetLeft(ellipse),
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
            fromValue: Canvas.GetTop(ellipse),
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

    /// <summary>
    /// Moves an ellipse to its starting position before the animation begins.
    /// </summary>
    /// <param name="ellipse">The ellipse to animate.</param>
    /// <param name="left">The target position on the canvas.</param>
    /// <param name="sb">The storyboard managing the animation.</param>
    private static void GoToStartPositionAnimation(Ellipse? ellipse, int left, Storyboard sb)
    {
        var leftEllipseAnim = new DoubleAnimation(
            fromValue: Canvas.GetLeft(ellipse),
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
    /// Removes the animation effects from an ellipse and resets its properties.
    /// </summary>
    /// <param name="ellipse">The ellipse to reset.</param>
    private static void RemoveEllipseAnimation(Ellipse ellipse)
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
    /// Starts the orbiting animation of an ellipse, creating a circular movement effect.
    /// </summary>
    /// <param name="ellipse">The ellipse to animate.</param>
    /// <param name="timeOffset">Delay before the animation starts.</param>
    /// <param name="fromBack">Indicates whether the animation should play in reverse.</param>
    private static void StartOrbitingAnimation(Ellipse ellipse, int timeOffset, bool fromBack)
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
    #endregion
}
