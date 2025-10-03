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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswScrollBar Orientation="Vertical" IsDynamic="True" CollapsedWidth="5" ExpandedWidth="15"/&gt;
/// </code>
/// </example>
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

        _border = GetTemplateChild("PART_Border") as Border;

        if (Orientation == Orientation.Vertical)
        {
            _arrowButton1 = GetTemplateChild("PART_LineUpButton") as ButtonBase;
            _arrowButton2 = GetTemplateChild("PART_LineDownButton") as ButtonBase;
        }
        else
        {
            _arrowButton1 = GetTemplateChild("PART_LineLeftButton") as ButtonBase;
            _arrowButton2 = GetTemplateChild("PART_LineRightButton") as ButtonBase;
        }

        OnDynamicModeChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <inheritdoc/>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (DynamicMode != StswScrollDynamicMode.Off)
            MouseEnterAnimation();
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (DynamicMode != StswScrollDynamicMode.Off)
            MouseLeaveAnimation();
    }

    /// <inheritdoc/>
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        if (DynamicMode == StswScrollDynamicMode.Full)
            ValueChangedAnimation();
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the scroll bar is dynamic (automatically hides when not in use).
    /// When set to true, the scroll bar will dynamically change its visibility and width based on user interaction.
    /// </summary>
    public StswScrollDynamicMode DynamicMode
    {
        get => (StswScrollDynamicMode)GetValue(DynamicModeProperty);
        set => SetValue(DynamicModeProperty, value);
    }
    public static readonly DependencyProperty DynamicModeProperty
        = DependencyProperty.Register(
            nameof(DynamicMode),
            typeof(StswScrollDynamicMode),
            typeof(StswScrollBar),
            new PropertyMetadata(default(StswScrollDynamicMode), OnDynamicModeChanged)
        );
    public static void OnDynamicModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswScrollBar stsw)
            return;

        stsw.StopAllAnimations();

        if (stsw.DynamicMode == StswScrollDynamicMode.Off)
        {
            stsw.SetSize(stsw.ExpandedSize);
            stsw.Opacity = 1;
            stsw._border?.SetCurrentValue(OpacityProperty, 1d);
            stsw._arrowButton1?.SetCurrentValue(OpacityProperty, 1d);
            stsw._arrowButton2?.SetCurrentValue(OpacityProperty, 1d);
        }
        else
        {
            stsw.SetSize(stsw.CollapsedSize);
            stsw.Opacity = stsw.DynamicMode == StswScrollDynamicMode.Full ? 0 : 1;
            var showDecor = stsw.DynamicMode == StswScrollDynamicMode.Off;
            stsw._border?.SetCurrentValue(OpacityProperty, showDecor ? 1d : 0d);
            stsw._arrowButton1?.SetCurrentValue(OpacityProperty, showDecor ? 1d : 0d);
            stsw._arrowButton2?.SetCurrentValue(OpacityProperty, showDecor ? 1d : 0d);
        }
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
    #endregion

    #region Animations
    /// <summary>
    /// Animates the opacity of the scroll bar to fade out after a specified delay.
    /// </summary>
    private void AnimateOpacityWithDelayedFadeOut()
    {
        if (!StswSettings.Default.EnableAnimations || DynamicMode != StswScrollDynamicMode.Full)
        {
            SetOpacity(this, DynamicMode == StswScrollDynamicMode.Full ? 0 : 1);
            return;
        }

        var hold = new DoubleAnimation(1, TimeSpan.FromMilliseconds(500))
        {
            BeginTime = TimeSpan.Zero,
            FillBehavior = FillBehavior.HoldEnd
        };

        var storyboard = new Storyboard();

        var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
        {
            BeginTime = TimeSpan.FromSeconds(5),
            EasingFunction = new CubicEase(),
            FillBehavior = FillBehavior.HoldEnd
        };

        Storyboard.SetTarget(hold, this);
        Storyboard.SetTargetProperty(hold, new PropertyPath(OpacityProperty));
        Storyboard.SetTarget(fade, this);
        Storyboard.SetTargetProperty(fade, new PropertyPath(OpacityProperty));

        storyboard.Children.Add(hold);
        storyboard.Children.Add(fade);
        storyboard.Begin();
    }

    /// <summary>
    /// Animates the opacity of the specified UI element to the target value.
    /// </summary>
    /// <param name="element">The UI element to animate.</param>
    /// <param name="toValue">The target opacity value.</param>
    /// <param name="duration">The duration of the animation.</param>
    /// <param name="delay">An optional delay before the animation starts.</param>
    private void AnimateOpacity(UIElement? element, double toValue, TimeSpan duration, TimeSpan? delay = null)
    {
        if (!StswSettings.Default.EnableAnimations || element == null)
        {
            SetOpacity(element, toValue);
            return;
        }

        var anim = new DoubleAnimation(toValue, duration)
        {
            EasingFunction = new CubicEase(),
            FillBehavior = FillBehavior.HoldEnd
        };

        element.BeginAnimation(OpacityProperty, anim);
    }

    /// <summary>
    /// Animates the size of the scroll bar to the specified value.
    /// </summary>
    /// <param name="toValue">The target size value to animate to.</param>
    private void AnimateSize(double toValue)
    {
        if (!StswSettings.Default.EnableAnimations)
        {
            SetSize(toValue);
            return;
        }

        var anim = new DoubleAnimation(toValue, TimeSpan.FromMilliseconds(500))
        {
            EasingFunction = new CubicEase(),
            FillBehavior = FillBehavior.HoldEnd
        };

        var prop = Orientation == Orientation.Horizontal ? HeightProperty : WidthProperty;
        BeginAnimation(prop, anim);
    }

    /// <summary>
    /// Animates the control when the mouse enters.
    /// </summary>
    private void MouseEnterAnimation()
    {
        AnimateSize(ExpandedSize);
        AnimateOpacity(this, 1, TimeSpan.FromMilliseconds(500));
        AnimateOpacity(_border, 1, TimeSpan.FromMilliseconds(500));
        AnimateOpacity(_arrowButton1, 1, TimeSpan.FromMilliseconds(500));
        AnimateOpacity(_arrowButton2, 1, TimeSpan.FromMilliseconds(500));
    }

    /// <summary>
    /// Animates the control when the mouse leaves.
    /// </summary>
    private void MouseLeaveAnimation()
    {
        if (Orientation == Orientation.Horizontal)
            Height = ExpandedSize;
        else
            Width = ExpandedSize;

        AnimateSize(CollapsedSize);
        AnimateOpacity(_border, 0, TimeSpan.FromMilliseconds(500));
        AnimateOpacity(_arrowButton1, 0, TimeSpan.FromMilliseconds(500));
        AnimateOpacity(_arrowButton2, 0, TimeSpan.FromMilliseconds(500));
        AnimateOpacityWithDelayedFadeOut();
    }

    /// <summary>
    /// Stops all animations on the scroll bar and its components.
    /// </summary>
    private void StopAllAnimations()
    {
        BeginAnimation(OpacityProperty, null);
        BeginAnimation(HeightProperty, null);
        BeginAnimation(WidthProperty, null);
        _border?.BeginAnimation(OpacityProperty, null);
        _arrowButton1?.BeginAnimation(OpacityProperty, null);
        _arrowButton2?.BeginAnimation(OpacityProperty, null);
    }

    /// <summary>
    /// Sets the opacity of the specified UI element to a given value, stopping any existing animations.
    /// </summary>
    /// <param name="element">The UI element whose opacity is to be set.</param>
    /// <param name="value">The opacity value to set (0.0 to 1.0).</param>
    private void SetOpacity(UIElement? element, double value)
    {
        if (element != null)
        {
            element.BeginAnimation(OpacityProperty, null);
            element.Opacity = value;
        }
    }

    /// <summary>
    /// Sets the size of the scroll bar based on its orientation.
    /// </summary>
    /// <param name="value">The size value to set.</param>
    private void SetSize(double value)
    {
        var property = Orientation == Orientation.Horizontal ? HeightProperty : WidthProperty;
        BeginAnimation(property, null);

        if (Orientation == Orientation.Horizontal)
            Height = value;
        else
            Width = value;
    }

    /// <summary>
    /// Animates the control when the value changes.
    /// </summary>
    private void ValueChangedAnimation()
    {
        if (IsMouseOver)
            return;

        AnimateOpacity(this, 1, TimeSpan.FromMilliseconds(300));
        AnimateOpacityWithDelayedFadeOut();
    }
    #endregion
}
