using System.Windows;

namespace TestApp;

public class StswMenuContext : ControlsContext
{
    public StswCommand<FrameworkElement> OnClickCommand => new((obj) => obj!.ContextMenu.IsOpen = true);
}
