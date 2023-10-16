using System.Windows.Input;
using System;

namespace TestApp;

public class StswNumericBoxContext : ControlsContext
{
    public StswCommand ClearCommand { get; set; }
    public StswCommand RandomizeCommand { get; set; }

    public StswNumericBoxContext()
    {
        ClearCommand = new(Clear);
        RandomizeCommand = new(Randomize);
    }

    #region Events and methods
    /// Command: clear
    private void Clear() => SelectedValue = default;

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
    private decimal increment = 1;
    public decimal Increment
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
    private decimal? maximum;
    public decimal? Maximum
    {
        get => maximum;
        set => SetProperty(ref maximum, value);
    }
    /// Minimum
    private decimal? minimum;
    public decimal? Minimum
    {
        get => minimum;
        set => SetProperty(ref minimum, value);
    }

    /// SelectedValue
    private decimal? selectedValue = 0;
    public decimal? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }
    #endregion
}
