using System;
using System.ComponentModel;
using System.Linq;

namespace TestApp;

public class StswPickerContext : ControlsContext
{
    /// Items
    private BindingList<StswListBoxTestModel> items = new(Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 }).ToList());
    public BindingList<StswListBoxTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
}
