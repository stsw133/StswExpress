using System;
using System.ComponentModel;
using System.Linq;

namespace TestApp;

public class StswPickerContext : ControlsContext
{
    /// Items
    public BindingList<StswListBoxTestModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private BindingList<StswListBoxTestModel> _items = new(Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 }).ToList());
}
