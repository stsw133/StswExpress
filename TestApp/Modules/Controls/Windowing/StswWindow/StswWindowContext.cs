using System.Windows.Data;
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
        var window = new StswWindow()
        {
            Title = "New window",
            Height = 450,
            Width = 750
        };
        window.SetBinding(StswWindow.IsEnabledProperty, new Binding()
        {
            Path = new System.Windows.PropertyPath(nameof(IsEnabled)),
            Source = this
        });
        window.Show();
    }
    #endregion
}
