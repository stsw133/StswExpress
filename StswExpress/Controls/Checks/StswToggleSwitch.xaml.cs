using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace StswExpress;

public class StswToggleSwitch : ToggleButton
{
    static StswToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleSwitch), new FrameworkPropertyMetadata(typeof(StswToggleSwitch)));
    }

    #region Parts

    Grid switchGrid;
    Border switchBorder;
    Border outerBorder;
    Path path;
    Viewbox checkedPath;
    Viewbox uncheckedPath;

    #endregion

    #region Private Properties

    Thickness checkedMargin => outerBorder != null ? new Thickness(outerBorder.ActualWidth-(BorderThickness.Left + BorderThickness.Right + switchGrid.ActualWidth + SwitchMargin.Right + SwitchMargin.Left),0,0,0) : new Thickness(0);
    Thickness uncheckedMargin => new Thickness(0);
    Thickness indeterminateMargin => outerBorder != null ? new Thickness(outerBorder.ActualWidth / 2 - (switchGrid.ActualWidth / 2 + BorderThickness.Left), 0,0,0) : new Thickness(0);

    #endregion

    #region OnApplyTemplate

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        switchGrid = GetTemplateChild("PART_SwitchGrid") as Grid;
        switchBorder = GetTemplateChild("PART_SwitchBorder") as Border;
        outerBorder = GetTemplateChild("PART_OuterBorder") as Border;
        path = GetTemplateChild("PART_Path") as Path;
        checkedPath = GetTemplateChild("PART_CheckedPath") as Viewbox;
        uncheckedPath = GetTemplateChild("PART_UncheckedPath") as Viewbox;

        path.Opacity = IsChecked == true ? 1 : 0;

        Loaded += OnLoaded;
        PreviewMouseDown += OnMouseDown;
        PreviewMouseUp += OnMouseUp;
        Checked += OnChecked;
        Unchecked += OnUnchecked;
        Indeterminate += OnIndeterminate;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Style properties

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswToggleSwitch),
            new PropertyMetadata(new CornerRadius(10)));

    public Thickness SwitchMargin
    {
        get { return (Thickness)GetValue(SwitchMarginProperty); }
        set { SetValue(SwitchMarginProperty, value); }
    }
    public static readonly DependencyProperty SwitchMarginProperty =
        DependencyProperty.Register(
            nameof(SwitchMargin),
            typeof(Thickness),
            typeof(StswToggleSwitch),
            new PropertyMetadata(new Thickness(4)));

    #endregion

    #region Main properties

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

    public Brush? CheckedGlyphBrush
    {
        get => (Brush?)GetValue(CheckedGlyphBrushProperty);
        set => SetValue(CheckedGlyphBrushProperty, value);
    }
    public static readonly DependencyProperty CheckedGlyphBrushProperty
        = DependencyProperty.Register(
            nameof(CheckedGlyphBrush),
            typeof(Brush),
            typeof(StswCheckBox)
        );

    public Brush? UncheckedGlyphBrush
    {
        get => (Brush?)GetValue(UncheckedGlyphBrushProperty);
        set => SetValue(UncheckedGlyphBrushProperty, value);
    }
    public static readonly DependencyProperty UncheckedGlyphBrushProperty
        = DependencyProperty.Register(
            nameof(UncheckedGlyphBrush),
            typeof(Brush),
            typeof(StswCheckBox)
        );

    #endregion

    #region Methods and Events
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AdjustSize();
    }
    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        MouseDownAnimation();
    }
    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        MouseUpAnimation();
    }
    private void OnChecked(object sender, RoutedEventArgs e)
    {
        CheckedAnimation();
    }
    private void OnUnchecked(object sender, RoutedEventArgs e)
    {
        UncheckedAnimation();
    }
    private void OnIndeterminate(object sender, RoutedEventArgs e)
    {
        IndeterminateAnimation();
    }
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        AdjustSize();
    }

    void AdjustSize()
    {
        switchBorder.CornerRadius = new CornerRadius(
            CornerRadius.TopLeft - BorderThickness.Top - SwitchMargin.Top,
            CornerRadius.TopRight - BorderThickness.Top - SwitchMargin.Top,
            CornerRadius.BottomLeft - BorderThickness.Bottom - SwitchMargin.Bottom,
            CornerRadius.BottomRight - BorderThickness.Bottom - SwitchMargin.Bottom);

        switch (IsChecked)
        {
            case true:
                switchGrid.Margin = checkedMargin;
                break;
            case false:
                switchGrid.Margin = uncheckedMargin;
                break;
            case null:
                switchGrid.Margin = indeterminateMargin;
                break;
        }
    }

    #endregion

    #region Animations

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
