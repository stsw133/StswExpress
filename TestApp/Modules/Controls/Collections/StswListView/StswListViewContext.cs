using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswListViewContext : ControlsContext
{
    public StswListViewContext()
    {
        Items.ListChanged += (_, _) => NotifyPropertyChanged(nameof(SelectionCounter));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectionMode = (SelectionMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? SelectionMode.Multiple;//default;
    }

    /// Items
    public BindingList<StswListBoxTestModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private BindingList<StswListBoxTestModel> _items = new(Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 }).ToList());

    /// SelectionCounter
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);

    /// SelectionMode
    public SelectionMode SelectionMode
    {
        get => _selectionMode;
        set => SetProperty(ref _selectionMode, value);
    }
    private SelectionMode _selectionMode;
}
