using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtCheckBox.xaml
/// </summary>
public partial class ExtCheckBox : CheckBox
{
    public ExtCheckBox()
    {
        InitializeComponent();
    }

    /// ID
    public static readonly DependencyProperty IDProperty
        = DependencyProperty.Register(
              nameof(ID),
              typeof(int),
              typeof(ExtCheckBox),
              new PropertyMetadata(default(int))
          );
    public int ID
    {
        get => (int)GetValue(IDProperty);
        set => SetValue(IDProperty, value);
    }
}
