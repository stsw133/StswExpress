﻿using System;

namespace TestApp;

public class StswDateBoxContext : StswObservableObject
{
    private DateTime date = DateTime.Now;
    public DateTime Date
    {
        get => date;
        set => SetProperty(ref date, value);
    }
}
