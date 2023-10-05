using System;

namespace TestApp;

public class StswAdaptiveBoxContext : ControlsContext
{
    public StswCommand SetRandomValueCommand { get; set; }

    public StswAdaptiveBoxContext()
    {
        SetRandomValueCommand = new(SetRandomValue);
        SetRandomValue();
    }

    #region Events and methods
    /// Command: make random value
    private void SetRandomValue()
    {
        var rand = 1 + new Random().Next(Enum.GetValues(typeof(StswAdaptiveType)).Length - 1);

        SelectedValue = rand switch
        {
            (int)StswAdaptiveType.Check => (bool?)true,
            (int)StswAdaptiveType.Date => (DateTime?)new DateTime().AddDays(new Random().Next((DateTime.MaxValue - DateTime.MinValue).Days)),
            (int)StswAdaptiveType.List => null,
            (int)StswAdaptiveType.Number => (double?)Convert.ToDouble(new Random().Next(int.MinValue, int.MaxValue)),
            (int)StswAdaptiveType.Text => (string?)Guid.NewGuid().ToString(),
            _ => null
        };
    }
    #endregion

    #region Properties
    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// SelectedValue
    private object? selectedValue;
    public object? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }
    #endregion
}
