using System.Windows;

namespace TestApp;

public class StswSidePanelContext : ControlsContext
{
    public StswCommand MinimizeCommand => new(() => StswApp.StswWindow.WindowState = WindowState.Minimized);
    public StswCommand MaximizeCommand => new(() => StswApp.StswWindow.WindowState = WindowState.Maximized);
    public StswCommand RestoreCommand => new(() => StswApp.StswWindow.WindowState = WindowState.Normal);
    public StswCommand FullscreenCommand => new(() => StswApp.StswWindow.Fullscreen = !StswApp.StswWindow.Fullscreen);
    public StswCommand CloseCommand => new(() => MessageBox.Show("Here's a random message instead of closing window."));
}
