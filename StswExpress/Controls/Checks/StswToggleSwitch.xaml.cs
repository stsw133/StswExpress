using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress;

public class StswToggleSwitch : ToggleButton
{
    static StswToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleSwitch), new FrameworkPropertyMetadata(typeof(StswToggleSwitch)));
    }

    #region Events & methods
    private Border? switchBorder, mainBorder;
    private Grid? switchGrid;
    private StswIcon? path, checkedPath, uncheckedPath;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_SwitchBorder") is Border switchBorder)
            this.switchBorder = switchBorder;
        if (GetTemplateChild("PART_MainBorder") is Border mainBorder)
            this.mainBorder = mainBorder;
        if (GetTemplateChild("PART_SwitchGrid") is Grid switchGrid)
            this.switchGrid = switchGrid;
        if (GetTemplateChild("PART_Icon") is StswIcon path)
        {
            path.Opacity = (IsChecked == true ? 1 : 0);
            this.path = path;
        }
        if (GetTemplateChild("PART_CheckedPath") is StswIcon checkedPath)
            this.checkedPath = checkedPath;
        if (GetTemplateChild("PART_UncheckedPath") is StswIcon uncheckedPath)
            this.uncheckedPath = uncheckedPath;

        Loaded += (s, e) => AdjustSize();
        SizeChanged += (s, e) => AdjustSize();
    }

    private Thickness checkedMargin => mainBorder != null ? new Thickness(mainBorder.ActualWidth - (BorderThickness.Left + BorderThickness.Right + switchGrid.ActualWidth + Padding.Right + Padding.Left), 0, 0, 0) : new Thickness(0);
    private Thickness uncheckedMargin => new Thickness(0);
    private Thickness indeterminateMargin => mainBorder != null ? new Thickness(mainBorder.ActualWidth / 2 - (switchGrid.ActualWidth / 2 + BorderThickness.Left), 0, 0, 0) : new Thickness(0);
    /*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        MouseDownAnimation();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        MouseUpAnimation();
    }
    */
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        CheckedAnimation();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        UncheckedAnimation();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnIndeterminate(RoutedEventArgs e)
    {
        base.OnIndeterminate(e);
        IndeterminateAnimation();
    }

    /// <summary>
    /// 
    /// </summary>
    private void AdjustSize()
    {
        if (switchBorder != null)
            switchBorder.CornerRadius = new CornerRadius(
                    CornerRadius.TopLeft - BorderThickness.Top - Padding.Top,
                    CornerRadius.TopRight - BorderThickness.Top - Padding.Top,
                    CornerRadius.BottomLeft - BorderThickness.Bottom - Padding.Bottom,
                    CornerRadius.BottomRight - BorderThickness.Bottom - Padding.Bottom
                );

        if (switchGrid != null)
            switchGrid.Margin = IsChecked switch
            {
                false => uncheckedMargin,
                true => checkedMargin,
                null => indeterminateMargin
            };
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
    /// Gets or sets the degree to which the corners of the control are rounded.
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
    /// Gets or sets the brush used to render the glyph (icon).
    /// </summary>
    public Brush? GlyphBrush
    {
        get => (Brush?)GetValue(GlyphBrushProperty);
        set => SetValue(GlyphBrushProperty, value);
    }
    public static readonly DependencyProperty GlyphBrushProperty
        = DependencyProperty.Register(
            nameof(GlyphBrush),
            typeof(Brush),
            typeof(StswToggleSwitch)
        );

    /// <summary>
    /// Gets or sets the brush used to render the glyph (icon).
    /// </summary>
    public Brush? CheckedGlyphBrush
    {
        get => (Brush?)GetValue(CheckedGlyphBrushProperty);
        set => SetValue(CheckedGlyphBrushProperty, value);
    }
    public static readonly DependencyProperty CheckedGlyphBrushProperty
        = DependencyProperty.Register(
            nameof(CheckedGlyphBrush),
            typeof(Brush),
            typeof(StswToggleSwitch)
        );

    /// <summary>
    /// Gets or sets the brush used to render the glyph (icon).
    /// </summary>
    public Brush? UncheckedGlyphBrush
    {
        get => (Brush?)GetValue(UncheckedGlyphBrushProperty);
        set => SetValue(UncheckedGlyphBrushProperty, value);
    }
    public static readonly DependencyProperty UncheckedGlyphBrushProperty
        = DependencyProperty.Register(
            nameof(UncheckedGlyphBrush),
            typeof(Brush),
            typeof(StswToggleSwitch)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon.
    /// </summary>
    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(Geometry),
            typeof(StswToggleSwitch)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon in the checked state.
    /// </summary>
    public Geometry? IconChecked
    {
        get => (Geometry?)GetValue(IconCheckedProperty);
        set => SetValue(IconCheckedProperty, value);
    }
    public static readonly DependencyProperty IconCheckedProperty
        = DependencyProperty.Register(
            nameof(IconChecked),
            typeof(Geometry),
            typeof(StswToggleSwitch)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon in the unchecked state.
    /// </summary>
    public Geometry? IconUnchecked
    {
        get => (Geometry?)GetValue(IconUncheckedProperty);
        set => SetValue(IconUncheckedProperty, value);
    }
    public static readonly DependencyProperty IconUncheckedProperty
        = DependencyProperty.Register(
            nameof(IconUnchecked),
            typeof(Geometry),
            typeof(StswToggleSwitch)
        );
    #endregion

    #region Animations
    /// <summary>
    /// 
    /// </summary>
    void MouseDownAnimation()
    {
        var sb = new Storyboard();

        var switchMargin = new ThicknessAnimation(
            toValue: new Thickness(switchGrid.ActualHeight * 0.05),
            duration: TimeSpan.FromMilliseconds(300));
        switchMargin.EasingFunction = new CubicEase();
        sb.Children.Add(switchMargin);
        Storyboard.SetTarget(switchMargin, switchBorder);
        Storyboard.SetTargetProperty(switchMargin, new PropertyPath(MarginProperty));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    void MouseUpAnimation()
    {
        var sb = new Storyboard();

        var switchMargin = new ThicknessAnimation(
            toValue: new Thickness(0),
            duration: TimeSpan.FromMilliseconds(300));
        switchMargin.EasingFunction = new CubicEase();
        sb.Children.Add(switchMargin);
        Storyboard.SetTarget(switchMargin, switchBorder);
        Storyboard.SetTargetProperty(switchMargin, new PropertyPath(MarginProperty));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckedAnimation()
    {
        var sb = new Storyboard();

        var switchColor = new ColorAnimation(
            toValue: ((SolidColorBrush)FindResource("StswToggle.Checked.Marker.Background")).Color,
            duration: TimeSpan.FromMilliseconds(300));
        switchColor.EasingFunction = new CubicEase();
        sb.Children.Add(switchColor);
        Storyboard.SetTarget(switchColor, switchBorder);
        Storyboard.SetTargetProperty(switchColor, new PropertyPath("(Border.Background).(SolidColorBrush.Color)", null));

        var switchPlacement = new ThicknessAnimation(
            toValue: checkedMargin,
            duration: TimeSpan.FromMilliseconds(300));
        switchPlacement.EasingFunction = new CubicEase();
        sb.Children.Add(switchPlacement);
        Storyboard.SetTarget(switchPlacement, switchGrid);
        Storyboard.SetTargetProperty(switchPlacement, new PropertyPath(MarginProperty));

        var pathOpacity = new DoubleAnimation(
            toValue: 1,
            duration: TimeSpan.FromMilliseconds(300));
        pathOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(pathOpacity);
        Storyboard.SetTarget(pathOpacity, path);
        Storyboard.SetTargetProperty(pathOpacity, new PropertyPath(OpacityProperty));

        var checkedContentOpacity = new DoubleAnimation(
            toValue: 1,
            duration: TimeSpan.FromMilliseconds(350));
        checkedContentOpacity.BeginTime = TimeSpan.FromMilliseconds(0);
        checkedContentOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(checkedContentOpacity);
        Storyboard.SetTarget(checkedContentOpacity, checkedPath);
        Storyboard.SetTargetProperty(checkedContentOpacity, new PropertyPath(OpacityProperty));

        var uncheckedContentOpacity = new DoubleAnimation(
            toValue: 0,
            duration: TimeSpan.FromMilliseconds(150));
        uncheckedContentOpacity.BeginTime = TimeSpan.FromMilliseconds(0);
        uncheckedContentOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(uncheckedContentOpacity);
        Storyboard.SetTarget(uncheckedContentOpacity, uncheckedPath);
        Storyboard.SetTargetProperty(uncheckedContentOpacity, new PropertyPath(OpacityProperty));

        var checkedContentSlide = new ThicknessAnimation(
            toValue: new Thickness(0),
            fromValue: new Thickness(ActualHeight, 0 ,-ActualHeight, 0),
            duration: TimeSpan.FromMilliseconds(350));
        checkedContentSlide.BeginTime = TimeSpan.FromMilliseconds(0);
        checkedContentSlide.EasingFunction = new CubicEase();
        sb.Children.Add(checkedContentSlide);
        Storyboard.SetTarget(checkedContentSlide, checkedPath);
        Storyboard.SetTargetProperty(checkedContentSlide, new PropertyPath(MarginProperty));

        sb.Begin();

    }

    /// <summary>
    /// 
    /// </summary>
    void UncheckedAnimation()
    {
        var sb = new Storyboard();

        var switchColor = new ColorAnimation(
            toValue: ((SolidColorBrush)FindResource("StswToggle.Unchecked.Marker.Background")).Color,
            duration: TimeSpan.FromMilliseconds(300));
        switchColor.EasingFunction = new CubicEase();
        sb.Children.Add(switchColor);
        Storyboard.SetTarget(switchColor, switchBorder);
        Storyboard.SetTargetProperty(switchColor, new PropertyPath("(Border.Background).(SolidColorBrush.Color)", null));

        var switchPlacement = new ThicknessAnimation(
            toValue: uncheckedMargin,
            duration: TimeSpan.FromMilliseconds(300));
        switchPlacement.EasingFunction = new CubicEase();
        sb.Children.Add(switchPlacement);
        Storyboard.SetTarget(switchPlacement, switchGrid);
        Storyboard.SetTargetProperty(switchPlacement, new PropertyPath(MarginProperty));

        var pathOpacity = new DoubleAnimation(
            toValue: 0,
            duration: TimeSpan.FromMilliseconds(300));
        pathOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(pathOpacity);
        Storyboard.SetTarget(pathOpacity, path);
        Storyboard.SetTargetProperty(pathOpacity, new PropertyPath(OpacityProperty));

        var checkedContentOpacity = new DoubleAnimation(
            toValue: 0,
            duration: TimeSpan.FromMilliseconds(150));
        checkedContentOpacity.BeginTime = TimeSpan.FromMilliseconds(0);
        checkedContentOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(checkedContentOpacity);
        Storyboard.SetTarget(checkedContentOpacity, checkedPath);
        Storyboard.SetTargetProperty(checkedContentOpacity, new PropertyPath(OpacityProperty));

        var uncheckedContentOpacity = new DoubleAnimation(
            toValue: 1,
            duration: TimeSpan.FromMilliseconds(350));
        uncheckedContentOpacity.BeginTime = TimeSpan.FromMilliseconds(0);
        uncheckedContentOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(uncheckedContentOpacity);
        Storyboard.SetTarget(uncheckedContentOpacity, uncheckedPath);
        Storyboard.SetTargetProperty(uncheckedContentOpacity, new PropertyPath(OpacityProperty));

        var uncheckedContentSlide = new ThicknessAnimation(
            toValue: new Thickness(0),
            fromValue: new Thickness(-ActualHeight, 0, ActualHeight, 0),
            duration: TimeSpan.FromMilliseconds(350));
        uncheckedContentSlide.BeginTime = TimeSpan.FromMilliseconds(0);
        uncheckedContentSlide.EasingFunction = new CubicEase();
        sb.Children.Add(uncheckedContentSlide);
        Storyboard.SetTarget(uncheckedContentSlide, uncheckedPath);
        Storyboard.SetTargetProperty(uncheckedContentSlide, new PropertyPath(MarginProperty));

        sb.Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    void IndeterminateAnimation()
    {
        var sb = new Storyboard();

        var switchColor = new ColorAnimation(
            toValue: ((SolidColorBrush)FindResource("StswToggle.Indeterminate.Marker.Background")).Color,
            duration: TimeSpan.FromMilliseconds(300));
        switchColor.EasingFunction = new CubicEase();
        sb.Children.Add(switchColor);
        Storyboard.SetTarget(switchColor, switchBorder);
        Storyboard.SetTargetProperty(switchColor, new PropertyPath("(Border.Background).(SolidColorBrush.Color)", null));

        var switchPlacement = new ThicknessAnimation(
            toValue: indeterminateMargin,
            duration: TimeSpan.FromMilliseconds(300));
        switchPlacement.EasingFunction = new CubicEase();
        sb.Children.Add(switchPlacement);
        Storyboard.SetTarget(switchPlacement, switchGrid);
        Storyboard.SetTargetProperty(switchPlacement, new PropertyPath(MarginProperty));

        var pathOpacity = new DoubleAnimation(
            toValue: 0,
            duration: TimeSpan.FromMilliseconds(300));
        pathOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(pathOpacity);
        Storyboard.SetTarget(pathOpacity, path);
        Storyboard.SetTargetProperty(pathOpacity, new PropertyPath(OpacityProperty));

        var checkedContentOpacity = new DoubleAnimation(
            toValue: 0,
            duration: TimeSpan.FromMilliseconds(150));
        checkedContentOpacity.BeginTime = TimeSpan.FromMilliseconds(0);
        checkedContentOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(checkedContentOpacity);
        Storyboard.SetTarget(checkedContentOpacity, checkedPath);
        Storyboard.SetTargetProperty(checkedContentOpacity, new PropertyPath(OpacityProperty));

        var uncheckedContentOpacity = new DoubleAnimation(
            toValue: 0,
            duration: TimeSpan.FromMilliseconds(150));
        uncheckedContentOpacity.BeginTime = TimeSpan.FromMilliseconds(0);
        uncheckedContentOpacity.EasingFunction = new CubicEase();
        sb.Children.Add(uncheckedContentOpacity);
        Storyboard.SetTarget(uncheckedContentOpacity, uncheckedPath);
        Storyboard.SetTargetProperty(uncheckedContentOpacity, new PropertyPath(OpacityProperty));

        sb.Begin();
    }
    #endregion
}
