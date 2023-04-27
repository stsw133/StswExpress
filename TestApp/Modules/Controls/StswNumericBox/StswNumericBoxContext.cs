using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswNumericBoxContext : StswObservableObject
{
    private double number1 = 0;
    public double Number1
    {
        get => number1;
        set => SetProperty(ref number1, value);
    }
    private double number2 = 0;
    public double Number2
    {
        get => number2;
        set => SetProperty(ref number2, value);
    }
    private double number3 = 0;
    public double Number3
    {
        get => number3;
        set => SetProperty(ref number3, value);
    }

    /// ...
    public ICommand SearchCommand { get; set; }

    public StswNumericBoxContext()
    {
        SearchCommand = new StswRelayCommand(Search);
    }

    /// SearchCommand
    private void Search()
    {
        MessageBox.Show(Number3.ToString());
    }
}
