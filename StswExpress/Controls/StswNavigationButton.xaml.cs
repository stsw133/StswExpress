using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswNavigationButton.xaml
/// </summary>
public partial class StswNavigationButton : RadioButton
{
    public StswNavigationButton()
    {
        InitializeComponent();
    }
    static StswNavigationButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationButton), new FrameworkPropertyMetadata(typeof(StswNavigationButton)));
    }

    #region Style
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }

    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }

    /// BackgroundPressed
    public static readonly DependencyProperty BackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundPressed),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundPressed
    {
        get => (Brush)GetValue(BackgroundPressedProperty);
        set => SetValue(BackgroundPressedProperty, value);
    }
    /// BorderBrushPressed
    public static readonly DependencyProperty BorderBrushPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushPressed),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushPressed
    {
        get => (Brush)GetValue(BorderBrushPressedProperty);
        set => SetValue(BorderBrushPressedProperty, value);
    }

    /// BackgroundChecked
    public static readonly DependencyProperty BackgroundCheckedProperty
        = DependencyProperty.Register(
            nameof(BackgroundChecked),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundChecked
    {
        get => (Brush)GetValue(BackgroundCheckedProperty);
        set => SetValue(BackgroundCheckedProperty, value);
    }
    /// BorderBrushChecked
    public static readonly DependencyProperty BorderBrushCheckedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushChecked),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushChecked
    {
        get => (Brush)GetValue(BorderBrushCheckedProperty);
        set => SetValue(BorderBrushCheckedProperty, value);
    }

    /// BorderBrushDefaulted
    public static readonly DependencyProperty BorderBrushDefaultedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDefaulted),
            typeof(Brush),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushDefaulted
    {
        get => (Brush)GetValue(BorderBrushDefaultedProperty);
        set => SetValue(BorderBrushDefaultedProperty, value);
    }
    #endregion

    #region Properties
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// CreateNewInstance
    public static readonly DependencyProperty CreateNewInstanceProperty
        = DependencyProperty.Register(
            nameof(CreateNewInstance),
            typeof(bool),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(bool))
        );
    public bool CreateNewInstance
    {
        get => (bool)GetValue(CreateNewInstanceProperty);
        set => SetValue(CreateNewInstanceProperty, value);
    }

    /// PageNamespace
    public static readonly DependencyProperty PageNamespaceProperty
        = DependencyProperty.Register(
            nameof(PageNamespace),
            typeof(string),
            typeof(StswNavigationButton),
            new PropertyMetadata(default(string))
        );
    public string PageNamespace
    {
        get => (string)GetValue(PageNamespaceProperty);
        set => SetValue(PageNamespaceProperty, value);
    }
    #endregion
}
