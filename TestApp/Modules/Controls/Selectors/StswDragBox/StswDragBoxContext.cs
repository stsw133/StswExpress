using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswDragBoxContext : ControlsContext
{
    public StswDragBoxContext()
    {
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(SelectionCounter));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        SelectionMode = (SelectionMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? default;
    }

    /// Items
    public ObservableCollection<StswListBoxTestModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private ObservableCollection<StswListBoxTestModel> _items = new(Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 }).ToList());

    //Items 2
    public ObservableCollection<StswListBoxTestModel> Items2
    {
        get => _items2;
        set => SetProperty(ref _items2, value);
    }
    private ObservableCollection<StswListBoxTestModel> _items2 = new(Enumerable.Range(16, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 }).ToList());

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
