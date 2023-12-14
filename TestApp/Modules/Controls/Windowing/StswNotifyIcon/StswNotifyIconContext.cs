using System.Windows;

namespace TestApp;

public class StswNotifyIconContext : ControlsContext
{
    public StswCommand TestCommand => new(() => MessageBox.Show("TEST"));
}
