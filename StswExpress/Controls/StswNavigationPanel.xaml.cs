using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswNavigationPanel.xaml
/// </summary>
public partial class StswNavigationPanel : StswNavigationPanelBase
{
    public StswNavigationPanel()
    {
        InitializeComponent();
    }
    static StswNavigationPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationPanel), new FrameworkPropertyMetadata(typeof(StswNavigationPanel)));
    }
}

public class StswNavigationPanelBase : UserControl
{
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNavigationPanelBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
