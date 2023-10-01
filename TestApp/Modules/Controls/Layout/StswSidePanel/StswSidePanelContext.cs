using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswSidePanelContext : ControlsContext
{
    public ICommand MinimizeCommand { get; set; }
    public ICommand MaximizeCommand { get; set; }
    public ICommand RestoreCommand { get; set; }
    public ICommand FullscreenCommand { get; set; }
    public ICommand CloseCommand { get; set; }

    public StswSidePanelContext()
    {
        MinimizeCommand = new StswCommand(Minimize);
        MaximizeCommand = new StswCommand(Maximize);
        RestoreCommand = new StswCommand(Restore);
        FullscreenCommand = new StswCommand(Fullscreen);
        CloseCommand = new StswCommand(Close);
    }

    #region Events and methods
    /// Command: minimize
    private void Minimize() => StswApp.StswWindow.WindowState = WindowState.Minimized;

    /// Command: maximize
    private void Maximize() => StswApp.StswWindow.WindowState = WindowState.Maximized;

    /// Command: restore
    private void Restore() => StswApp.StswWindow.WindowState = WindowState.Normal;

    /// Command: fullscreen
    private void Fullscreen() => StswApp.StswWindow.Fullscreen = !StswApp.StswWindow.Fullscreen;

    /// Command: close
    private void Close() => MessageBox.Show("Here's a random message instead of closing window.");
    #endregion
}
