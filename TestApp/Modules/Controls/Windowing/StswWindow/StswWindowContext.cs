using System.Windows.Input;

namespace TestApp;

public class StswWindowContext : ControlsContext
{
    public ICommand OpenNewWindowCommand { get; set; }

    public StswWindowContext()
    {
        OpenNewWindowCommand = new StswCommand(OpenNewWindow);
    }

    #region Events and methods
    /// Command: open new window
    private void OpenNewWindow()
    {
        new StswWindow()
        {
            Title = "New window",
            Height = 450,
            Width = 750
        }.Show();
    }
    #endregion
}
