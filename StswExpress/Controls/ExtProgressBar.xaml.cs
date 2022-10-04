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
