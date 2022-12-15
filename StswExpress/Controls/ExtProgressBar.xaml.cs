using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtProgressBar.xaml
/// </summary>
public partial class ExtProgressBar : ProgressBar
{
    public ExtProgressBar()
    {
        InitializeComponent();
    }

    /// TextSymbol
    public static readonly DependencyProperty TextSymbolProperty
        = DependencyProperty.Register(
              nameof(TextSymbol),
              typeof(string),
              typeof(ExtProgressBar),
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
              typeof(ExtProgressBar),
              new PropertyMetadata(default(Visibility))
          );
    public Visibility TextVisibility
    {
        get => (Visibility)GetValue(TextVisibilityProperty);
        set => SetValue(TextVisibilityProperty, value);
    }
}
