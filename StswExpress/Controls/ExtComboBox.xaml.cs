using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtComboBox.xaml
/// </summary>
public partial class ExtComboBox : ComboBox
{
    public ExtComboBox()
    {
        InitializeComponent();
    }

    /// ID
    public static readonly DependencyProperty IDProperty
        = DependencyProperty.Register(
              nameof(ID),
              typeof(int),
              typeof(ExtComboBox),
              new PropertyMetadata(default(int))
          );
    public int ID
    {
        get => (int)GetValue(IDProperty);
        set => SetValue(IDProperty, value);
    }
}
