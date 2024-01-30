using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress;

public class StswToggleSwitch : ToggleButton, IStswCornerControl
{
    static StswToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleSwitch), new FrameworkPropertyMetadata(typeof(StswToggleSwitch)));
    }

    #region Events & methods
    private Border? _switch;
    private Border? _outerBorder;
    private Border? _checkedBackgroundBorder;

    private bool loaded = false;
    private double _width = 0;
    private double _height = 0;
    private double _switchSize = 0;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Switch") is Border _switch)
            this._switch = _switch;
        if (GetTemplateChild("PART_MainBorder") is Border _outerBorder)
            this._outerBorder = _outerBorder;
        if (GetTemplateChild("PART_CheckBackground") is Border _checkedBackgroundBorder)
            this._checkedBackgroundBorder = _checkedBackgroundBorder;

        Loaded += (s, e) => SetSwitch();
        loaded = true;
    }

    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        if (_switch != null)
            AnimateChecked();
    }

    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        if (_switch != null)
            AnimateUnhecked();
    }

    protected override void OnIndeterminate(RoutedEventArgs e)
    {
        base.OnIndeterminate(e);
        if (_switch != null)
            AnimateIndeterminate();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        if (!loaded)
            return;

        _width = _outerBorder!.RenderSize.Width;
        _height = _outerBorder!.RenderSize.Height;
        _switchSize = _switch!.RenderSize.Height;

        SetSwitch();
    }

    void SetSwitch()
    {
        _switch!.Measure(new Size(ActualWidth, ActualHeight));
        _switch!.Width = _switchSize;
        _switch!.Measure(new Size(ActualWidth, ActualHeight));
        _switch.CornerRadius = new CornerRadius(_switchSize * 1.2 / 2.0);
        InstantSwitch();
        _checkedBackgroundBorder!.Margin = new Thickness(-Padding.Left, -Padding.Top, -Padding.Right, -Padding.Bottom);
        _checkedBackgroundBorder!.CornerRadius = new CornerRadius(_height / 2.0);
        _outerBorder!.CornerRadius = new CornerRadius(_height / 2.0);
        if (IsChecked == true)
            _checkedBackgroundBorder.Opacity = 1;
        else
            _checkedBackgroundBorder.Opacity = 0;
    }

    Thickness SwitchPadding(bool? state)
    {
        Thickness padding;
        switch (state)
        {
            case false:
                padding = new Thickness(0);
                break;
            case true:
                padding = new Thickness(_width - _outerBorder!.BorderThickness.Left - _outerBorder.BorderThickness.Right - _outerBorder.Padding.Left - _outerBorder.Padding.Right - _switchSize,
                    0, 0, 0);
                break;
            default:
                padding = new Thickness((_width - _outerBorder!.BorderThickness.Left - _outerBorder.BorderThickness.Right - _outerBorder.Padding.Left - _outerBorder.Padding.Right - _switchSize) / 2,
                    0, 0, 0);
                break;
        }
        return padding;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the scale of the icon in the box.
    /// </summary>
    public GridLength IconScale
    {
        get => (GridLength)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength),
            typeof(StswToggleSwitch)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswToggleSwitch)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswToggleSwitch)
        );

    /// <summary>
    /// 
    /// </summary>
    public Brush CheckedBrush
    {
        get => (Brush)GetValue(CheckedBrushProperty);
        set => SetValue(CheckedBrushProperty, value);
    }
    public static readonly DependencyProperty CheckedBrushProperty
        = DependencyProperty.Register(
            nameof(CheckedBrush),
            typeof(Brush),
            typeof(StswToggleSwitch),
            new PropertyMetadata(new LinearGradientBrush(Color.FromRgb(0, 130, 130), Color.FromRgb(0, 170, 170), 0.8))
        );

    #endregion

    #region Animations
    void InstantSwitch()
    {
        var sb = new Storyboard();

        var switchMarginAnim = new ThicknessAnimation(
            toValue: SwitchPadding(IsChecked),
            TimeSpan.Zero,
            FillBehavior.HoldEnd);
        sb.Children.Add(switchMarginAnim);
        Storyboard.SetTarget(switchMarginAnim, _switch);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var switchWidthAnim = new DoubleAnimation(
            toValue: _switchSize,
            TimeSpan.Zero,
            FillBehavior.HoldEnd);
        sb.Children.Add(switchWidthAnim);
        Storyboard.SetTarget(switchWidthAnim, _switch);
        Storyboard.SetTargetProperty(switchWidthAnim, new PropertyPath(WidthProperty));

        sb.Begin();
    }


    void AnimateChecked()
    {
        var sb = new Storyboard();

        var switchMarginAnim = new ThicknessAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = SwitchPadding(true),
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(switchMarginAnim);
        Storyboard.SetTarget(switchMarginAnim, _switch);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 1,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _checkedBackgroundBorder);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    void AnimateUnhecked()
    {
        var sb = new Storyboard();

        var switchMarginAnim = new ThicknessAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = SwitchPadding(false),
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(switchMarginAnim);
        Storyboard.SetTarget(switchMarginAnim, _switch);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 0,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _checkedBackgroundBorder);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    void AnimateIndeterminate()
    {
        var sb = new Storyboard();

        var switchMarginAnim = new ThicknessAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = SwitchPadding(null),
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(switchMarginAnim);
        Storyboard.SetTarget(switchMarginAnim, _switch);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 0,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _checkedBackgroundBorder);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }
    #endregion
}
