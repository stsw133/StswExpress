using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswGroupBox.xaml
/// </summary>
public partial class StswGroupBox : GroupBox
{
    public StswGroupBox()
    {
        InitializeComponent();
    }
    static StswGroupBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGroupBox), new FrameworkPropertyMetadata(typeof(StswGroupBox)));
    }

    #region Properties
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswGroupBox)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion

    #region Style
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswGroupBox)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    /// SubCornerRadius
    public static readonly DependencyProperty SubCornerRadiusProperty
        = DependencyProperty.Register(
            nameof(SubCornerRadius),
            typeof(CornerRadius),
            typeof(StswGroupBox)
        );
    public CornerRadius SubCornerRadius
    {
        get => (CornerRadius)GetValue(SubCornerRadiusProperty);
        set => SetValue(SubCornerRadiusProperty, value);
    }
    /// SubHorizontalAlignment
    public static readonly DependencyProperty SubHorizontalAlignmentProperty
        = DependencyProperty.Register(
            nameof(SubHorizontalAlignment),
            typeof(HorizontalAlignment),
            typeof(StswGroupBox)
        );
    public HorizontalAlignment SubHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(SubHorizontalAlignmentProperty);
        set => SetValue(SubHorizontalAlignmentProperty, value);
    }
    /// SubMargin
    public static readonly DependencyProperty SubMarginProperty
        = DependencyProperty.Register(
            nameof(SubMargin),
            typeof(Thickness),
            typeof(StswGroupBox)
        );
    public Thickness SubMargin
    {
        get => (Thickness)GetValue(SubMarginProperty);
        set => SetValue(SubMarginProperty, value);
    }
    #endregion
}
