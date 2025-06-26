using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress;
/// <summary>
/// A toggle switch control that provides an animated on/off state transition.
/// Supports custom brushes, animations, and read-only mode.
/// </summary>
/// <remarks>
/// The control includes built-in animations for smooth state transitions, 
/// as well as an optional read-only mode to prevent user interaction.
/// </remarks>
public class StswToggleSwitch : ToggleButton, IStswCornerControl
{
    private Border? _mainBorder, _backgroundBorder, _circleBorder;
    private double _height = 0, _width = 0, _switchSize = 0;
    private bool _isLoaded = false;

    static StswToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleSwitch), new FrameworkPropertyMetadata(typeof(StswToggleSwitch)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _mainBorder = GetTemplateChild("PART_MainBorder") as Border;
        _backgroundBorder = GetTemplateChild("PART_BackgroundBorder") as Border;
        _circleBorder = GetTemplateChild("PART_CircleBorder") as Border;

        Loaded += (_, _) => SetSwitch();
        _isLoaded = true;
    }

    /// <inheritdoc/>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        if (_circleBorder != null)
            AnimateChecked();
    }

    /// <inheritdoc/>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        if (_circleBorder != null)
            AnimateUnhecked();
    }

    /// <inheritdoc/>
    protected override void OnIndeterminate(RoutedEventArgs e)
    {
        base.OnIndeterminate(e);
        if (_circleBorder != null)
            AnimateIndeterminate();
    }

    /// <inheritdoc/>
    protected override void OnToggle()
    {
        if (!IsReadOnly)
            base.OnToggle();
    }

    /// <inheritdoc/>
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
    /// Initializes and updates the visual properties of the toggle switch.
    /// This method ensures correct positioning, padding, and styling of elements
    /// based on the current state and dimensions of the control.
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
    /// Calculates the padding for the toggle switch based on its current state.
    /// This determines the position of the circular toggle button.
    /// </summary>
    /// <param name="state">
    /// The state of the switch: <see langword="true"/> for checked, 
    /// <see langword="false"/> for unchecked, or <see langword="null"/> for indeterminate.
    /// </param>
    /// <returns>
    /// A <see cref="Thickness"/> value representing the calculated padding for the toggle button.
    /// </returns>
    Thickness SwitchPadding(bool? state) => state switch
    {
        false => new Thickness(0),
        true => new Thickness(_width - _mainBorder!.BorderThickness.Left - _mainBorder.BorderThickness.Right - Padding.Left - Padding.Right - _switchSize, 0, 0, 0),
        _ => new Thickness((_width - _mainBorder!.BorderThickness.Left - _mainBorder.BorderThickness.Right - Padding.Left - Padding.Right - _switchSize) / 2, 0, 0, 0),
    };
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the scale of the switch's toggle button.
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
    /// Gets or sets a value indicating whether the toggle switch is in read-only mode.
    /// When set to <see langword="true"/>, the switch cannot be toggled.
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
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
    /// Gets or sets the brush used to render the toggle button.
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
    /// Instantly sets the switch position without animations.
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
    /// Animates the switch to the checked state.
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
    /// Animates the switch to the unchecked state.
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
    /// Animates the switch to the indeterminate (centered) state.
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

/* usage:

<se:StswToggleSwitch Content="Dark Mode" IsChecked="True"/>

*/
