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
[Stsw("0.1.0", Changes = StswPlannedChanges.None)]
public class StswScrollBar : ScrollBar
{
    private ButtonBase? _arrowButton1, _arrowButton2;
    private Border? _border;

    static StswScrollBar()
	{
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswScrollBar), new FrameworkPropertyMetadata(typeof(StswScrollBar)));
    }

    #region Events & methods
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
            if (GetTemplateChild("PART_LineUpButton") is ButtonBase btnUp)
                _arrowButton1 = btnUp;
            if (GetTemplateChild("PART_LineDownButton") is ButtonBase btnDown)
                _arrowButton2 = btnDown;
        }
        else
        {
            if (GetTemplateChild("PART_LineLeftButton") is ButtonBase btnLeft)
                _arrowButton1 = btnLeft;
            if (GetTemplateChild("PART_LineRightButton") is ButtonBase btnRight)
                _arrowButton2 = btnRight;
        }

        /// IsDynamic
        OnIsDynamicChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <inheritdoc/>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (IsDynamic)
            MouseEnterAnimation();
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (IsDynamic)
            MouseLeaveAnimation();
    }

    /// <inheritdoc/>
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        if (IsDynamic)
            ValueChangedAnimation();
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the size of the scroll bar when collapsed.
    /// This size is applied when the scroll bar is not being interacted with (in dynamic mode).
    /// </summary>
    public double CollapsedSize
    {
        get => (double)GetValue(CollapsedSizeProperty);
        set => SetValue(CollapsedSizeProperty, value);
    }
    public static readonly DependencyProperty CollapsedSizeProperty
        = DependencyProperty.Register(
            nameof(CollapsedSize),
            typeof(double),
            typeof(StswScrollBar),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Gets or sets the size of the scroll bar when expanded.
    /// This size is applied when the scroll bar is actively being used or hovered over in dynamic mode.
    /// </summary>
    public double ExpandedSize
    {
        get => (double)GetValue(ExpandedSizeProperty);
        set => SetValue(ExpandedSizeProperty, value);
    }
    public static readonly DependencyProperty ExpandedSizeProperty
        = DependencyProperty.Register(
            nameof(ExpandedSize),
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
            var newSize = stsw.IsDynamic ? stsw.CollapsedSize : stsw.ExpandedSize;

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
            toValue: ExpandedSize,
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
            toValue: CollapsedSize,
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
