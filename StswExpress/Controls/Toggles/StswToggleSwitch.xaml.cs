using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress;
/// <summary>
/// 
/// </summary>
public class StswToggleSwitch : ToggleButton, IStswCornerControl
{
    static StswToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleSwitch), new FrameworkPropertyMetadata(typeof(StswToggleSwitch)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswToggleSwitch), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private Border? _mainBorder, _backgroundBorder, _circleBorder;

    private bool _isLoaded = false;
    private double _width = 0;
    private double _height = 0;
    private double _switchSize = 0;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _mainBorder = GetTemplateChild("PART_MainBorder") as Border;
        _backgroundBorder = GetTemplateChild("PART_BackgroundBorder") as Border;
        _circleBorder = GetTemplateChild("PART_CircleBorder") as Border;

        Loaded += (_, _) => SetSwitch();
        _isLoaded = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        if (_circleBorder != null)
            AnimateChecked();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        if (_circleBorder != null)
            AnimateUnhecked();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnIndeterminate(RoutedEventArgs e)
    {
        base.OnIndeterminate(e);
        if (_circleBorder != null)
            AnimateIndeterminate();
    }

    /// <summary>
    /// Prevents state changes when the <see cref="IsReadOnly"/> property is set to <see langword="true"/>.
    /// </summary>
    protected override void OnToggle()
    {
        if (!IsReadOnly)
            base.OnToggle();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sizeInfo"></param>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        if (!_isLoaded)
            return;

        _width = _mainBorder!.RenderSize.Width;
        _height = _mainBorder!.RenderSize.Height;
        _switchSize = _circleBorder!.RenderSize.Height;

        SetSwitch();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetSwitch()
    {
        if (_mainBorder != null)
        {
            _circleBorder!.Measure(new Size(_mainBorder.ActualWidth, _mainBorder.ActualHeight));
            _circleBorder!.Width = _switchSize;
            _circleBorder!.Measure(new Size(_mainBorder.ActualWidth, _mainBorder.ActualHeight));
            _circleBorder.CornerRadius = new CornerRadius(_switchSize * 1.2 / 2.0);
            InstantSwitch();
            _backgroundBorder!.Margin = new Thickness(-Padding.Left, -Padding.Top, -Padding.Right, -Padding.Bottom);
            _backgroundBorder!.CornerRadius = new CornerRadius(_height / 2.0);
            _mainBorder!.CornerRadius = new CornerRadius(_height / 2.0);
            if (IsChecked == true)
                _backgroundBorder.Opacity = 1;
            else
                _backgroundBorder.Opacity = 0;
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

    #region Logic properties
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

    /// <summary>
    /// Gets or sets a value indicating whether the control is in read-only mode.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
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
            typeof(StswToggleSwitch),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswToggleSwitch),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswToggleSwitch),
            new FrameworkPropertyMetadata(default(Brush?), FrameworkPropertyMetadataOptions.AffectsRender)
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
        Storyboard.SetTarget(switchMarginAnim, _circleBorder);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var switchWidthAnim = new DoubleAnimation(
            toValue: _switchSize,
            TimeSpan.Zero,
            FillBehavior.HoldEnd);
        sb.Children.Add(switchWidthAnim);
        Storyboard.SetTarget(switchWidthAnim, _circleBorder);
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
        Storyboard.SetTarget(switchMarginAnim, _circleBorder);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 1,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _backgroundBorder);
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
        Storyboard.SetTarget(switchMarginAnim, _circleBorder);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 0,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _backgroundBorder);
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
        Storyboard.SetTarget(switchMarginAnim, _circleBorder);
        Storyboard.SetTargetProperty(switchMarginAnim, new PropertyPath(MarginProperty));

        var backgroundOpacityAnim = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillBehavior = FillBehavior.HoldEnd,
            To = 0,
            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
        };
        sb.Children.Add(backgroundOpacityAnim);
        Storyboard.SetTarget(backgroundOpacityAnim, _backgroundBorder);
        Storyboard.SetTargetProperty(backgroundOpacityAnim, new PropertyPath(OpacityProperty));

        sb.Begin();
    }
    #endregion
}
