using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace StswExpress;

public class RgsScrollBar : ScrollBar
{
    #region Parts
    private Border border;
    private RepeatButton arrowButton1;
    private RepeatButton arrowButton2;
    #endregion

    #region Constructor
    static RgsScrollBar()
	{
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RgsScrollBar), new FrameworkPropertyMetadata(typeof(RgsScrollBar)));
    }
    #endregion

    #region OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        border = GetTemplateChild("PART_Border") as Border;

        border.Opacity = 0;

        if (Orientation == Orientation.Vertical)
        {
            Width = CollapsedWidth;
            arrowButton1 = GetTemplateChild("PART_LineUpButton") as RepeatButton;
            arrowButton2 = GetTemplateChild("PART_LineDownButton") as RepeatButton;
        }
        else
        {
            Height = CollapsedWidth;
            arrowButton1 = GetTemplateChild("PART_LineLeftButton") as RepeatButton;
            arrowButton2 = GetTemplateChild("PART_LineRightButton") as RepeatButton;
        }
        arrowButton1.Opacity = 0;
        arrowButton2.Opacity = 0;

        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
    }
    #endregion

    #region Dependency Properties

    #region ExpandedWidth
    public static readonly DependencyProperty ExpandedWidthProperty =
        DependencyProperty.Register(nameof(ExpandedWidth), typeof(double), typeof(RgsScrollBar), new PropertyMetadata(15.0));
    public double ExpandedWidth
    {
        get { return (double)GetValue(ExpandedWidthProperty); }
        set { SetValue(ExpandedWidthProperty, value); }
    }
    #endregion

    #region CollapsedWidth
    public static readonly DependencyProperty CollapsedWidthProperty =
        DependencyProperty.Register(nameof(CollapsedWidth), typeof(double), typeof(RgsScrollBar), new PropertyMetadata(7.0));
    public double CollapsedWidth
    {
        get { return (double)GetValue(CollapsedWidthProperty); }
        set { SetValue(CollapsedWidthProperty, value); }
    }
    #endregion

    #endregion

    #region EventHandlers
    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        MouseEnterAnimation();
    }
    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        MouseLeaveAnimation();
    }
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);

        ValueChangedAnimation();
    }
    #endregion

    #region Animations
    private void MouseEnterAnimation()
    {
        var duration = TimeSpan.FromMilliseconds(500);

        var sb = new Storyboard();

        var widthAnim = new DoubleAnimation(
        toValue: ExpandedWidth,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        widthAnim.EasingFunction = new CubicEase();
        sb.Children.Add(widthAnim);
        Storyboard.SetTarget(widthAnim, this);
        if (Orientation == Orientation.Horizontal)
            Storyboard.SetTargetProperty(widthAnim, new PropertyPath(HeightProperty));
        else
            Storyboard.SetTargetProperty(widthAnim, new PropertyPath(WidthProperty));

        var backgroundOpacityAnim = new DoubleAnimation(
        toValue: 1,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        backgroundOpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, border);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        var arrow1OpacityAnim = new DoubleAnimation(
        toValue: 1,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        arrow1OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow1OpacityAnim);
        Storyboard.SetTarget(arrow1OpacityAnim, arrowButton1);
        Storyboard.SetTargetProperty(arrow1OpacityAnim, new PropertyPath(OpacityProperty));

        var arrow2OpacityAnim = new DoubleAnimation(
        toValue: 1,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        arrow2OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow2OpacityAnim);
        Storyboard.SetTarget(arrow2OpacityAnim, arrowButton2);
        Storyboard.SetTargetProperty(arrow2OpacityAnim, new PropertyPath(OpacityProperty));

        var opacityAnim = new DoubleAnimation(
        toValue: 1,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        opacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, this);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    private void MouseLeaveAnimation()
    {
        var duration = TimeSpan.FromMilliseconds(500);

        var sb = new Storyboard();

        var widthAnim = new DoubleAnimation(
        toValue: CollapsedWidth,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        widthAnim.EasingFunction = new CubicEase();
        sb.Children.Add(widthAnim);
        Storyboard.SetTarget(widthAnim, this);
        if (Orientation == Orientation.Horizontal)
            Storyboard.SetTargetProperty(widthAnim, new PropertyPath(HeightProperty));
        else
            Storyboard.SetTargetProperty(widthAnim, new PropertyPath(WidthProperty));

        var backgroundOpacityAnim = new DoubleAnimation(
        toValue: 0,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        backgroundOpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, border);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        var arrow1OpacityAnim = new DoubleAnimation(
        toValue: 0,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        arrow1OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow1OpacityAnim);
        Storyboard.SetTarget(arrow1OpacityAnim, arrowButton1);
        Storyboard.SetTargetProperty(arrow1OpacityAnim, new PropertyPath(OpacityProperty));

        var arrow2OpacityAnim = new DoubleAnimation(
        toValue: 0,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        arrow2OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow2OpacityAnim);
        Storyboard.SetTarget(arrow2OpacityAnim, arrowButton2);
        Storyboard.SetTargetProperty(arrow2OpacityAnim, new PropertyPath(OpacityProperty));

        var opacityAnim = new DoubleAnimation(
        toValue: 0,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        opacityAnim.EasingFunction = new CubicEase();
        opacityAnim.BeginTime = TimeSpan.FromSeconds(5);
        sb.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, this);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    private void ValueChangedAnimation()
    {
        if (IsMouseOver) return;

        var duration = TimeSpan.FromMilliseconds(300);

        var sb = new Storyboard();

        var opacityAnim = new DoubleAnimation(
        toValue: 1,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        opacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, this);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        var reverseOpacityAnim = new DoubleAnimation(
        toValue: 0,
        duration: duration,
        fillBehavior: FillBehavior.HoldEnd);
        reverseOpacityAnim.EasingFunction = new CubicEase();
        reverseOpacityAnim.BeginTime = TimeSpan.FromSeconds(5);
        sb.Children.Add(reverseOpacityAnim);
        Storyboard.SetTarget(reverseOpacityAnim, this);
        Storyboard.SetTargetProperty(reverseOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    #endregion
}
