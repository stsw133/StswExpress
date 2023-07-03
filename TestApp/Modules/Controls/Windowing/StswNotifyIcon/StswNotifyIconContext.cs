using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswNotifyIconContext : StswObservableObject
{
    public ICommand TestCommand { get; set; }

    public StswNotifyIconContext()
    {
        TestCommand = new StswRelayCommand(Test);
    }

    /// Command: test
    private void Test()
    {
        MessageBox.Show("TEST");
    }
}
