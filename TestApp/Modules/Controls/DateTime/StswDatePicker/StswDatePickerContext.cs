using System;

namespace TestApp;

public class StswDatePickerContext : StswObservableObject
{
    private DateTime date = DateTime.Now;
    public DateTime Date
    {
        get => date;
        set => SetProperty(ref date, value);
    }

    private DateTime? nullableDate;
    public DateTime? NullableDate
    {
        get => nullableDate;
        set => SetProperty(ref nullableDate, value);
    }
}
