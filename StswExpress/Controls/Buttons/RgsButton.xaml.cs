using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class RgsButton : Button
{
    #region Private
    private double _width;
    private double _height;
    //private double size => Math.Max(_width, _height) * Math.Sqrt(2);
    #endregion

    #region Constructor
    static RgsButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RgsButton), new FrameworkPropertyMetadata(typeof(RgsButton)));
    }
    #endregion

    #region OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PreviewMouseDown += OnPreviewMouseDown;
    }
    #endregion

    #region Methods & Events
    private void OnPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var point = e.GetPosition(this);

        var ellipse = new Ellipse()
        {
            Width = 0,
            Height = 0,
            Fill = new SolidColorBrush(Color.FromArgb(70, 120, 120, 120))
        };
        Canvas.SetLeft(ellipse, point.X - BorderThickness.Left);
        Canvas.SetTop(ellipse, point.Y - BorderThickness.Top);

        var size = new List<double>(){
            Math.Abs(_width - point.X),
            Math.Abs(point.X),
            Math.Abs(value: _height - point.Y),
            Math.Abs(point.Y)
        }.OrderByDescending(d => d).First();

        size = size * Math.Sqrt(2) * 1.1;

        Ellipses.Add(ellipse);

        AnimateEllipse(ellipse, size);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        _width = sizeInfo.NewSize.Width;
        _height = sizeInfo.NewSize.Height;
    }
    #endregion

    #region DP
    public ObservableCollection<UIElement> Ellipses
    {
        get { return (ObservableCollection<UIElement>)GetValue(EllipsesProperty); }
        set { SetValue(EllipsesProperty, value); }
    }
    public static readonly DependencyProperty EllipsesProperty =
        DependencyProperty.Register(nameof(Ellipses), typeof(ObservableCollection<UIElement>), typeof(RgsButton), 
            new PropertyMetadata(new ObservableCollection<UIElement>()));
    #endregion

    #region Animation
    private void AnimateEllipse(Ellipse ellipse, double size)
    {
        var sb = new Storyboard();

        var widthAnim = new DoubleAnimation(
           fromValue: 0,
           toValue: size * 2,
           duration: TimeSpan.FromMilliseconds(700),
           fillBehavior: FillBehavior.Stop
           )
        { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } };
        sb.Children.Add(widthAnim);
        Storyboard.SetTarget(widthAnim, ellipse);
        Storyboard.SetTargetProperty(widthAnim, new PropertyPath(WidthProperty));

        DoubleAnimation? heightAnim = new DoubleAnimation(
           fromValue: 0,
           toValue: size * 2,
           duration: TimeSpan.FromMilliseconds(700),
           fillBehavior: FillBehavior.Stop
           )
        { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } };
        sb.Children.Add(heightAnim);
        Storyboard.SetTarget(heightAnim, ellipse);
        Storyboard.SetTargetProperty(heightAnim, new PropertyPath(HeightProperty));

        ThicknessAnimation? marginAnim = new ThicknessAnimation(
           fromValue: new Thickness(0),
           toValue: new Thickness(-size),
           duration: TimeSpan.FromMilliseconds(700),
           fillBehavior: FillBehavior.Stop
           )
        { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } };
        sb.Children.Add(marginAnim);
        Storyboard.SetTarget(marginAnim, ellipse);
        Storyboard.SetTargetProperty(marginAnim, new PropertyPath(MarginProperty));

        DoubleAnimationUsingKeyFrames opacityAnim = new DoubleAnimationUsingKeyFrames()
        {
            Duration = TimeSpan.FromMilliseconds(900),
            KeyFrames = new DoubleKeyFrameCollection()
            {
                new DiscreteDoubleKeyFrame(0, KeyTime.FromPercent(0)),
                new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0.15), new CubicEase(){EasingMode=EasingMode.EaseOut}),
                new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(0.5)),
                new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1), new CubicEase(){EasingMode=EasingMode.EaseOut})
            },
            FillBehavior = FillBehavior.HoldEnd
        };
        sb.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, ellipse);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        sb.Completed += (s, e) =>
        {
            Ellipses.Remove(ellipse);
        };

        sb.Begin();
    }
    #endregion
}
