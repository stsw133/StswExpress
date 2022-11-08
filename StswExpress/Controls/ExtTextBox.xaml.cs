using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtTextBox.xaml
/// </summary>
public partial class ExtTextBox : TextBox
{
    public ExtTextBox()
    {
        InitializeComponent();
    }

    /// ID
    public static readonly DependencyProperty IDProperty
        = DependencyProperty.Register(
              nameof(ID),
              typeof(int),
              typeof(ExtTextBox),
              new PropertyMetadata(default(int))
          );
    public int ID
    {
        get => (int)GetValue(IDProperty);
        set => SetValue(IDProperty, value);
    }
}
