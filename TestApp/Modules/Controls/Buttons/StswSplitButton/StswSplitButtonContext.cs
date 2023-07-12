using System.Windows.Input;
using System.Windows;

namespace TestApp;

public class StswSplitButtonContext : StswObservableObject
{
    public ICommand ClickCommand { get; set; }

    public StswSplitButtonContext()
    {
        ClickCommand = new StswRelayCommand<string>(Click);
    }

    /// ClickCommand
    private void Click(string parameter)
    {
        MessageBox.Show(parameter);
    }
}
