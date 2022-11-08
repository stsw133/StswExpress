using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtToggleButton.xaml
/// </summary>
public partial class ExtToggleButton : ToggleButton
{
    public ExtToggleButton()
    {
        InitializeComponent();
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
              nameof(CornerRadius),
              typeof(double?),
              typeof(ExtToggleButton),
              new PropertyMetadata(default(double?))
          );
    public double? CornerRadius
    {
        get => (double?)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// ForMultiBox
    internal static readonly DependencyProperty ForMultiBoxProperty
        = DependencyProperty.Register(
              nameof(ForMultiBox),
              typeof(bool),
              typeof(ExtToggleButton),
              new PropertyMetadata(default(bool))
          );
    internal bool ForMultiBox
    {
        get => (bool)GetValue(ForMultiBoxProperty);
        set => SetValue(ForMultiBoxProperty, value);
    }

    /// ID
    public static readonly DependencyProperty IDProperty
        = DependencyProperty.Register(
              nameof(ID),
              typeof(int),
              typeof(ExtToggleButton),
              new PropertyMetadata(default(int))
          );
    public int ID
    {
        get => (int)GetValue(IDProperty);
        set => SetValue(IDProperty, value);
    }
}
