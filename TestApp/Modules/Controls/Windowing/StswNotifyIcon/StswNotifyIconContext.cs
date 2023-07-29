using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswNotifyIconContext : ControlsContext
{
    public ICommand TestCommand { get; set; }

    public StswNotifyIconContext()
    {
        TestCommand = new StswCommand(Test);
    }

    #region Events and methods
    /// Command: test
    private void Test()
    {
        MessageBox.Show("TEST");
    }
    #endregion
}
