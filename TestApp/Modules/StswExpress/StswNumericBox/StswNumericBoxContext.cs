using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswNumericBoxContext : StswObservableObject
{
    private double number = 0;
    public double Number
    {
        get => number;
        set => SetProperty(ref number, value);
    }

    /// ...
    public ICommand ClearCommand { get; set; }
    public ICommand SearchCommand { get; set; }

    public StswNumericBoxContext()
    {
        ClearCommand = new StswRelayCommand(Clear);
        SearchCommand = new StswRelayCommand(Search);
    }

    /// ClearCommand
    private void Clear()
    {
        Number = 0;
    }
    /// SearchCommand
    private void Search()
    {
        MessageBox.Show(Number.ToString());
    }
}
