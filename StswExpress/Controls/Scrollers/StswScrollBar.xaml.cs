using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace StswExpress;
/// <summary>
/// A <see cref="ScrollBar"/> extension with dynamic visibility and animated resizing.
/// Supports automatic expansion when hovered over.
/// </summary>
public class StswScrollBar : ScrollBar
{
    static StswScrollBar()
	{
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswScrollBar), new FrameworkPropertyMetadata(typeof(StswScrollBar)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswScrollBar), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private Border? _border;
    private RepeatButton? _arrowButton1, _arrowButton2;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Border
        if (GetTemplateChild("PART_Border") is Border border)
            _border = border;

        /// Orientation
        if (Orientation == Orientation.Vertical)
        {
            if (GetTemplateChild("PART_LineUpButton") is RepeatButton btnUp)
                _arrowButton1 = btnUp;
            if (GetTemplateChild("PART_LineDownButton") is RepeatButton btnDown)
                _arrowButton2 = btnDown;
        }
        else
        {
            if (GetTemplateChild("PART_LineLeftButton") is RepeatButton btnLeft)
                _arrowButton1 = btnLeft;
            if (GetTemplateChild("PART_LineRightButton") is RepeatButton btnRight)
                _arrowButton2 = btnRight;
        }

        /// IsDynamic
        OnIsDynamicChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Handles the MouseEnter event for dynamic behavior.
    /// Triggers an animation when the mouse enters the scroll bar, expanding its visibility and width if dynamic mode is enabled.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (IsDynamic)
            MouseEnterAnimation();
    }

    /// <summary>
    /// Handles the MouseLeave event for dynamic behavior.
    /// Triggers an animation to collapse the scroll bar's visibility and width when the mouse leaves, if dynamic mode is enabled.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (IsDynamic)
            MouseLeaveAnimation();
    }

    /// <summary>
    /// Handles the ValueChanged event for dynamic behavior.
    /// Triggers an animation when the value of the scroll bar changes, making it more noticeable if not hovering.
    /// </summary>
    /// <param name="oldValue">The old value</param>
    /// <param name="newValue">The new value</param>
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        if (IsDynamic)
            ValueChangedAnimation();
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the width of the scroll bar when collapsed.
    /// This width is applied when the scroll bar is not being interacted with (in dynamic mode).
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
            typeof(StswScrollBar),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Gets or sets the width of the scroll bar when expanded.
    /// This width is applied when the scroll bar is actively being used or hovered over in dynamic mode.
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
            typeof(StswScrollBar),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the scroll bar is dynamic (automatically hides when not in use).
    /// When set to true, the scroll bar will dynamically change its visibility and width based on user interaction.
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

            if (stsw._border != null)
                stsw._border.Opacity = newOpacity;
            if (stsw._arrowButton1 != null)
                stsw._arrowButton1.Opacity = newOpacity;
            if (stsw._arrowButton2 != null)
                stsw._arrowButton2.Opacity = newOpacity;
        }
    }
    #endregion

    #region Animations
    /// <summary>
    /// Animates the control when the mouse enters.
    /// Expands the scroll bar's width, increases opacity, and shows arrow buttons with a smooth animation.
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
        Storyboard.SetTarget(backgroundOpacityAnim, _border);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        var arrow1OpacityAnim = new DoubleAnimation(
            toValue: 1,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        arrow1OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow1OpacityAnim);
        Storyboard.SetTarget(arrow1OpacityAnim, _arrowButton1);
        Storyboard.SetTargetProperty(arrow1OpacityAnim, new PropertyPath(OpacityProperty));

        var arrow2OpacityAnim = new DoubleAnimation(
            toValue: 1,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        arrow2OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow2OpacityAnim);
        Storyboard.SetTarget(arrow2OpacityAnim, _arrowButton2);
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
    /// Animates the control when the mouse leaves.
    /// Collapses the scroll bar's width and hides the content (arrows and border) with a smooth animation.
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
        Storyboard.SetTarget(backgroundOpacityAnim, _border);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        var arrow1OpacityAnim = new DoubleAnimation(
            toValue: 0,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        arrow1OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow1OpacityAnim);
        Storyboard.SetTarget(arrow1OpacityAnim, _arrowButton1);
        Storyboard.SetTargetProperty(arrow1OpacityAnim, new PropertyPath(OpacityProperty));

        var arrow2OpacityAnim = new DoubleAnimation(
            toValue: 0,
            duration: duration,
            fillBehavior: FillBehavior.HoldEnd
        );
        arrow2OpacityAnim.EasingFunction = new CubicEase();
        sb.Children.Add(arrow2OpacityAnim);
        Storyboard.SetTarget(arrow2OpacityAnim, _arrowButton2);
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
    /// Animates the control when the value changes.
    /// Makes the scroll bar more visible for a short time and then fades out if not hovered over.
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

/* usage:

<se:StswScrollBar Orientation="Vertical" IsDynamic="True" CollapsedWidth="5" ExpandedWidth="15"/>

*/
