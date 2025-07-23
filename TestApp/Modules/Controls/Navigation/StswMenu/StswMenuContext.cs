using System.Windows;

namespace TestApp;
public partial class StswMenuContext : ControlsContext
{
    [StswCommand] void OnClick(FrameworkElement obj) => obj.ContextMenu.IsOpen = true;
}
