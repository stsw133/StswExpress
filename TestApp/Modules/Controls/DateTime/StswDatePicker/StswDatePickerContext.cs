using System;
using System.Windows.Input;

namespace TestApp;

public class StswDatePickerContext : ControlsContext
{
    public ICommand ClearCommand { get; set; }
    public ICommand RandomizeCommand { get; set; }

    public StswDatePickerContext()
    {
        ClearCommand = new StswCommand(Clear);
        RandomizeCommand = new StswCommand(Randomize);
    }

    #region Events and methods
    /// Command: clear
    private void Clear() => SelectedDate = default;

    /// Command: randomize
    private void Randomize() => SelectedDate = new DateTime().AddDays(new Random().Next((DateTime.MaxValue-DateTime.MinValue).Days));
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
    private string? format = "d";
    public string? Format
    {
        get => format;
        set => SetProperty(ref format, value);
    }

    /// IncrementType
    private StswDateIncrementType incrementType = StswDateIncrementType.Day;
    public StswDateIncrementType IncrementType
    {
        get => incrementType;
        set => SetProperty(ref incrementType, value);
    }

    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Maximum
    private DateTime? maximum;
    public DateTime? Maximum
    {
        get => maximum;
        set => SetProperty(ref maximum, value);
    }
    /// Minimum
    private DateTime? minimum;
    public DateTime? Minimum
    {
        get => minimum;
        set => SetProperty(ref minimum, value);
    }

    /// SelectedDate
    private DateTime? selectedDate = DateTime.Now;
    public DateTime? SelectedDate
    {
        get => selectedDate;
        set => SetProperty(ref selectedDate, value);
    }
    #endregion
}
