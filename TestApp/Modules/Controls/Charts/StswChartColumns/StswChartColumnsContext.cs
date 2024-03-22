﻿using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;

public class StswChartColumnsContext : ControlsContext
{
    public StswCommand AddValueCommand => new(() => {
        Items.First(x => x.Name == "Option 9").Value += 20;
        Items = Items.OrderByDescending(x => x.Value).ToObservableCollection();
    });

    /// Items
    private ObservableCollection<StswChartElementModel> items = new()
    {
        new() { Name = "Option 1", Value = 1000, Description = "The biggest (by default) source of value" },
        new() { Name = "Option 2", Value = 810 },
        new() { Name = "Option 3", Value = 640 },
        new() { Name = "Option 4", Value = 490 },
        new() { Name = "Option 5", Value = 360 },
        new() { Name = "Option 6", Value = 250 },
        new() { Name = "Option 7", Value = 160 },
        new() { Name = "Option 8", Value = 90 },
        new() { Name = "Option 9", Value = 40, Description = "Value of this source can be increased through button" },
        new() { Name = "Option 10", Value = 10, Description = "The smallest (by default) source of value" }
    };
    public ObservableCollection<StswChartElementModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
}
