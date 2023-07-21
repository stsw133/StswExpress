using System;

namespace TestApp;

public class StswCalendarContext : ControlsContext
{
    #region Properties
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
