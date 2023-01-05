using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswProgressBar.xaml
/// </summary>
public partial class StswProgressBar : ProgressBar
{
    public StswProgressBar()
    {
        InitializeComponent();
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswProgressBar),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// TextSymbol
    public static readonly DependencyProperty TextSymbolProperty
        = DependencyProperty.Register(
            nameof(TextSymbol),
            typeof(string),
            typeof(StswProgressBar),
            new PropertyMetadata("%")
        );
    public string? TextSymbol
    {
        get => (string?)GetValue(TextSymbolProperty);
        set => SetValue(TextSymbolProperty, value);
    }

    /// TextVisibility
    public static readonly DependencyProperty TextVisibilityProperty
        = DependencyProperty.Register(
            nameof(TextVisibility),
            typeof(Visibility),
            typeof(StswProgressBar),
            new PropertyMetadata(default(Visibility))
        );
    public Visibility TextVisibility
    {
        get => (Visibility)GetValue(TextVisibilityProperty);
        set => SetValue(TextVisibilityProperty, value);
    }
}
