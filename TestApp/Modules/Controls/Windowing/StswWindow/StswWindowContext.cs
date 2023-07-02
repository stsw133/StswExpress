using System.Windows.Input;

namespace TestApp;

public class StswWindowContext : StswObservableObject
{
    public ICommand OpenNewWindowCommand { get; set; }

    public StswWindowContext()
    {
        OpenNewWindowCommand = new StswRelayCommand(OpenNewWindow);
    }

    /// Command: open new window
    private void OpenNewWindow()
    {
        new StswWindow() { Height = 450, Width = 750, Title = "New window" }.Show();
    }
}
