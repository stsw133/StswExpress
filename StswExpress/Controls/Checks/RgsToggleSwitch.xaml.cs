using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StswExpress;

public class RgsToggleSwitch : ToggleButton
{
    #region Parts

    Border switchBorder;
    Border outerBorder;

    #endregion

    #region Constructor

    static RgsToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RgsToggleSwitch), new FrameworkPropertyMetadata(typeof(RgsToggleSwitch)));
    }

    #endregion

    #region OnApplyTemplate

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        switchBorder = GetTemplateChild("PART_SwitchBorder") as Border;
        outerBorder = GetTemplateChild("PART_OuterBorder") as Border;

        Checked += OnChecked;
        Unchecked += OnUnchecked;
        Indeterminate += OnIndeterminate;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Style Properties

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(RgsToggleSwitch),
            new PropertyMetadata(new CornerRadius(10)));
    #endregion

    #region Methods and Events

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
       
    }
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var x = ActualWidth;

        switchBorder.Width = 10;// (outerBorder.ActualWidth - (outerBorder.BorderThickness.Left + outerBorder.BorderThickness.Right)) / 2;

        switch (IsChecked)
        {
            case true:
                //switchBorder.Margin = new Thickness((outerBorder.ActualWidth - (outerBorder.BorderThickness.Left + outerBorder.BorderThickness.Right)) / 2);
                break;
            case false:
                //switchBorder.Margin = new Thickness((outerBorder.ActualWidth - (outerBorder.BorderThickness.Left + outerBorder.BorderThickness.Right)) / 2);
                break;
            case null:
                //switchBorder.Margin = new Thickness((outerBorder.ActualWidth - (outerBorder.BorderThickness.Left + outerBorder.BorderThickness.Right)) / 2);
                break;
        }
    }

    #endregion

    #region Animations

    void CheckedAnimation()
    {
        var sb = new Storyboard();

        var switchColor = new ColorAnimation(
            toValue: ((SolidColorBrush)FindResource("StswCheck.Checked.Static.Background")).Color,
            duration: TimeSpan.FromMilliseconds(300));
        sb.Children.Add(switchColor);
        Storyboard.SetTarget(switchColor, switchBorder);
        Storyboard.SetTargetProperty(switchColor, new PropertyPath("(Border.Background).(SolidColorBrush.Color)", null));

        sb.Begin();

    }

    void UncheckedAnimation()
    {
        var sb = new Storyboard();

        var switchColor = new ColorAnimation(
            toValue: ((SolidColorBrush)FindResource("StswCheck.Unchecked.Static.Background")).Color,
            duration: TimeSpan.FromMilliseconds(300));
        sb.Children.Add(switchColor);
        Storyboard.SetTarget(switchColor, switchBorder);
        Storyboard.SetTargetProperty(switchColor, new PropertyPath("(Border.Background).(SolidColorBrush.Color)", null));

        sb.Begin();

    }

    #endregion
}
