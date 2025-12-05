using System.Windows;

namespace TestApp.Wpf;
public partial class StswMenuContext : ControlsContext
{
    [StswCommand] void OnClick(FrameworkElement obj) => obj.ContextMenu.IsOpen = true;
}
