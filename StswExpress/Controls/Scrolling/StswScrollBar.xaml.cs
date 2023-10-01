using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswScrollBar : ScrollBar
{
    static StswScrollBar()
	{
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswScrollBar), new FrameworkPropertyMetadata(typeof(StswScrollBar)));
    }

    #region Events & methods
    private Border? border;
    private RepeatButton? arrowButton1;
    private RepeatButton? arrowButton2;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Border
        if (GetTemplateChild("PART_Border") is Border border)
            this.border = border;

        /// Orientation
        if (Orientation == Orientation.Vertical)
        {
            if (GetTemplateChild("PART_LineUpButton") is RepeatButton btnUp)
                arrowButton1 = btnUp;
            if (GetTemplateChild("PART_LineDownButton") is RepeatButton btnDown)
                arrowButton2 = btnDown;
        }
        else
        {
            if (GetTemplateChild("PART_LineLeftButton") is RepeatButton btnLeft)
                arrowButton1 = btnLeft;
            if (GetTemplateChild("PART_LineRightButton") is RepeatButton btnRight)
                arrowButton2 = btnRight;
        }

        /// IsDynamic
        OnIsDynamicChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (IsDynamic)
            MouseEnterAnimation();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (IsDynamic)
            MouseLeaveAnimation();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        if (IsDynamic)
            ValueChangedAnimation();
    }
    #endregion

    #region Style properties
    /// <summary>
    /// 
    /// </summary>
    public double CollapsedWidth
    {
        get => (double)GetValue(CollapsedWidthProperty);
        set => SetValue(CollapsedWidthProperty, value);
    }
    public static readonly DependencyProperty CollapsedWidthProperty
        = DependencyProperty.Register(
            nameof(CollapsedWidth),
            typeof(double),
            typeof(StswScrollBar)
        );

    /// <summary>
    /// 
    /// </summary>
    public double ExpandedWidth
    {
        get => (double)GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }
    public static readonly DependencyProperty ExpandedWidthProperty
        = DependencyProperty.Register(
            nameof(ExpandedWidth),
            typeof(double),
            typeof(StswScrollBar)
        );

    /// <summary>
    /// 
    /// </summary>
    public bool IsDynamic
    {
        get => (bool)GetValue(IsDynamicProperty);
        set => SetValue(IsDynamicProperty, value);
    }
    public static readonly DependencyProperty IsDynamicProperty
        = DependencyProperty.Register(
            nameof(IsDynamic),
            typeof(bool),
            typeof(StswScrollBar),
            new PropertyMetadata(default(bool), OnIsDynamicChanged)
        );
    public static void OnIsDynamicChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswScrollBar stsw)
        {
            /// height & width
            var newSize = stsw.IsDynamic ? stsw.CollapsedWidth : stsw.ExpandedWidth;

            if (stsw.Orientation == Orientation.Horizontal)
                stsw.Height = newSize;
            else
                stsw.Width = newSize;

            /// opacity
            var newOpacity = stsw.IsDynamic ? 0 : 1;

            if (stsw.border != null)
                stsw.border.Opacity = newOpacity;
            if (stsw.arrowButton1 != null)
                stsw.arrowButton1.Opacity = newOpacity;
            if (stsw.arrowButton2 != null)
                stsw.arrowButton2.Opacity = newOpacity;
        }
    }
    #endregion

    #region Animations
    /// <summary>
    /// 
    /// </summary>
    private void MouseEnterAnimation()
    {
        var duration = TimeSpan.FromMilliseconds(500);

        var sb = new Storyboard();

        var widthAnim = new DoubleAnimation(
            toValue: ExpandedWidth,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        widthAnim.EasingFunction = new CubicEase();
        sb.Children.Add(widthAnim);
        Storyboard.SetTarget(widthAnim, this);
        Storyboard.SetTargetProperty(widthAnim, new PropertyPath(Orientation == Orientation.Horizontal ? HeightProperty : WidthProperty));

        var backgroundOpacityAnim = new DoubleAnimation(
            toValue: 1,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        backgroundOpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, border);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        var arrow1OpacityAnim = new DoubleAnimation(
            toValue: 1,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        arrow1OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow1OpacityAnim);
        Storyboard.SetTarget(arrow1OpacityAnim, arrowButton1);
        Storyboard.SetTargetProperty(arrow1OpacityAnim, new PropertyPath(OpacityProperty));

        var arrow2OpacityAnim = new DoubleAnimation(
            toValue: 1,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        arrow2OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow2OpacityAnim);
        Storyboard.SetTarget(arrow2OpacityAnim, arrowButton2);
        Storyboard.SetTargetProperty(arrow2OpacityAnim, new PropertyPath(OpacityProperty));

        var opacityAnim = new DoubleAnimation(
            toValue: 1,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        opacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, this);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    private void MouseLeaveAnimation()
    {
        var duration = TimeSpan.FromMilliseconds(500);

        var sb = new Storyboard();

        var widthAnim = new DoubleAnimation(
            toValue: CollapsedWidth,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        widthAnim.EasingFunction = new CubicEase();
        sb.Children.Add(widthAnim);
        Storyboard.SetTarget(widthAnim, this);
        Storyboard.SetTargetProperty(widthAnim, new PropertyPath(Orientation == Orientation.Horizontal ? HeightProperty : WidthProperty));

        var backgroundOpacityAnim = new DoubleAnimation(
            toValue: 0,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        backgroundOpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, border);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        var arrow1OpacityAnim = new DoubleAnimation(
            toValue: 0,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        arrow1OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow1OpacityAnim);
        Storyboard.SetTarget(arrow1OpacityAnim, arrowButton1);
        Storyboard.SetTargetProperty(arrow1OpacityAnim, new PropertyPath(OpacityProperty));

        var arrow2OpacityAnim = new DoubleAnimation(
            toValue: 0,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        arrow2OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow2OpacityAnim);
        Storyboard.SetTarget(arrow2OpacityAnim, arrowButton2);
        Storyboard.SetTargetProperty(arrow2OpacityAnim, new PropertyPath(OpacityProperty));

        var opacityAnim = new DoubleAnimation(
            toValue: 0,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        opacityAnim.EasingFunction = new CubicEase();
        opacityAnim.BeginTime = TimeSpan.FromSeconds(5);
        sb.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, this);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    private void ValueChangedAnimation()
    {
        if (IsMouseOver)
            return;

        var duration = TimeSpan.FromMilliseconds(300);

        var sb = new Storyboard();

        var opacityAnim = new DoubleAnimation(
            toValue: 1,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        opacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, this);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        var reverseOpacityAnim = new DoubleAnimation(
            toValue: 0,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        reverseOpacityAnim.EasingFunction = new CubicEase();
        reverseOpacityAnim.BeginTime = TimeSpan.FromSeconds(5);
        sb.Children.Add(reverseOpacityAnim);
        Storyboard.SetTarget(reverseOpacityAnim, this);
        Storyboard.SetTargetProperty(reverseOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }
    #endregion
}
