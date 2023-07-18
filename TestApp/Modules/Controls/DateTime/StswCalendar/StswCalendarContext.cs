using System;

namespace TestApp;

public class StswCalendarContext : ControlsContext
{
    #region Properties
    /// Date
    private DateTime? date = DateTime.Now;
    public DateTime? Date
    {
        get => date;
        set => SetProperty(ref date, value);
    }
    #endregion
}
