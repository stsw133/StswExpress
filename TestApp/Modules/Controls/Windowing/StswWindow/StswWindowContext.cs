using System.Windows;
using System.Windows.Data;

namespace TestApp;
public partial class StswWindowContext : ControlsContext
{
    [StswCommand] void OpenNewWindow()
    {
        var window = new StswWindow()
        {
            Owner = StswApp.StswWindow,
            Title = "New window",
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Height = 450,
            Width = 750
        };
        window.SetBinding(UIElement.IsEnabledProperty, new Binding()
        {
            Path = new PropertyPath(nameof(IsEnabled)),
            Source = this
        });
        window.Show();
    }
}
