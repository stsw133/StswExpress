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
    private Border? _mainBorder, _checkedBorder, _switchBorder;

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

        if (GetTemplateChild("PART_MainBorder") is Border mainBorder)
            _mainBorder = mainBorder;
        if (GetTemplateChild("PART_CheckBorder") is Border checkedBorder)
            _checkedBorder = checkedBorder;
        if (GetTemplateChild("PART_SwitchBorder") is Border switchBorder)
            _switchBorder = switchBorder;

        Loaded += (s, e) => SetSwitch();
        loaded = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        if (_switchBorder != null)
            AnimateChecked();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        if (_switchBorder != null)
            AnimateUnhecked();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnIndeterminate(RoutedEventArgs e)
    {
        base.OnIndeterminate(e);
        if (_switchBorder != null)
            AnimateIndeterminate();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sizeInfo"></param>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        if (!loaded)
            return;

        _width = _mainBorder!.RenderSize.Width;
        _height = _mainBorder!.RenderSize.Height;
        _switchSize = _switchBorder!.RenderSize.Height;

        SetSwitch();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetSwitch()
    {
        if (_mainBorder != null)
        {
            _switchBorder!.Measure(new Size(_mainBorder.ActualWidth, _mainBorder.ActualHeight));
            _switchBorder!.Width = _switchSize;
            _switchBorder!.Measure(new Size(_mainBorder.ActualWidth, _mainBorder.ActualHeight));
            _switchBorder.CornerRadius = new CornerRadius(_switchSize * 1.2 / 2.0);
            InstantSwitch();
            _checkedBorder!.Margin = new Thickness(-Padding.Left, -Padding.Top, -Padding.Right, -Padding.Bottom);
            _checkedBorder!.CornerRadius = new CornerRadius(_height / 2.0);
            _mainBorder!.CornerRadius = new CornerRadius(_height / 2.0);
            if (IsChecked == true)
                _checkedBorder.Opacity = 1;
            else
                _checkedBorder.Opacity = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    Thickness SwitchPadding(bool? state) => state switch
    {
        false => new Thickness(0),
        true => new Thickness(_width - _mainBorder!.BorderThickness.Left - _mainBorder.BorderThickness.Right - Padding.Left - Padding.Right - _switchSize, 0, 0, 0),
        _ => new Thickness((_width - _mainBorder!.BorderThickness.Left - _mainBorder.BorderThickness.Right - Padding.Left - Padding.Right - _switchSize) / 2, 0, 0, 0),
    };
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
    /// Gets or sets the brush used to render the toggle.
    /// </summary>
    public Brush? ToggleBrush
    {
        get => (Brush?)GetValue(ToggleBrushProperty);
        set => SetValue(ToggleBrushProperty, value);
    }
    public static readonly DependencyProperty ToggleBrushProperty
        = DependencyProperty.Register(
            nameof(ToggleBrush),
            typeof(Brush),
            typeof(StswToggleSwitch)
        );
    #endregion

    #region Animations
    /// <summary>
    /// 
    /// </summary>
    void InstantSwitch()
    {
        var sb = new Storyboard();

        var switchMarginAnim = new ThicknessAnimation(
            toValue: SwitchPadding(IsChecked),
            TimeSpan.Zero,
            FillBehavior.HoldEnd);
        sb.Children.Add(switchMarginAnim);
        Storyboard.SetTarget(switchMarginAnim, _switchBorder);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var switchWidthAnim = new DoubleAnimation(
            toValue: _switchSize,
            TimeSpan.Zero,
            FillBehavior.HoldEnd);
        sb.Children.Add(switchWidthAnim);
        Storyboard.SetTarget(switchWidthAnim, _switchBorder);
        Storyboard.SetTargetProperty(switchWidthAnim, new PropertyPath(WidthProperty));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
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
        Storyboard.SetTarget(switchMarginAnim, _switchBorder);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 1,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _checkedBorder);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
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
        Storyboard.SetTarget(switchMarginAnim, _switchBorder);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 0,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _checkedBorder);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
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
        Storyboard.SetTarget(switchMarginAnim, _switchBorder);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 0,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _checkedBorder);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }
    #endregion
}
