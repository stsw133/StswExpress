using System.Windows.Input;
using System;

namespace TestApp;

public class StswNumericBoxContext : ControlsContext
{
    public ICommand RandomizeCommand { get; set; }

    public StswNumericBoxContext()
    {
        RandomizeCommand = new StswCommand(Randomize);
    }

    #region Events and methods
    /// Command: randomize
    private void Randomize() => SelectedValue = new Random().Next(int.MinValue, int.MaxValue);
    #endregion

    #region Properties
    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
    }

    /// Format
    private string? format = "N2";
    public string? Format
    {
        get => format;
        set => SetProperty(ref format, value);
    }

    /// Increment
    private double increment = 1;
    public double Increment
    {
        get => increment;
        set => SetProperty(ref increment, value);
    }

    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Maximum
    private double? maximum;
    public double? Maximum
    {
        get => maximum;
        set => SetProperty(ref maximum, value);
    }
    /// Minimum
    private double? minimum;
    public double? Minimum
    {
        get => minimum;
        set => SetProperty(ref minimum, value);
    }

    /// SelectedValue
    private double? selectedValue = 0;
    public double? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }
    #endregion
}
