using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswNavigationPanel.xaml
/// </summary>
public partial class StswNavigation : StswNavigationBase
{
    public StswNavigation()
    {
        InitializeComponent();
    }
    static StswNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigation), new FrameworkPropertyMetadata(typeof(StswNavigation)));
    }
}

public class StswNavigationBase : UserControl
{
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigationBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
