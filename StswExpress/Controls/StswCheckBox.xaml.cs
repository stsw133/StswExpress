using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswCheckBox.xaml
/// </summary>
public partial class StswCheckBox : CheckBox
{
    public StswCheckBox()
    {
        InitializeComponent();
    }
    /*
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius?),
            typeof(StswCheckBox),
            new PropertyMetadata(default(CornerRadius?))
        );
    public CornerRadius? CornerRadius
    {
        get => (CornerRadius?)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    */
}
