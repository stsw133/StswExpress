using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswSegmentContext : ControlsContext
{
    public StswSegmentContext()
    {
        Items.ListChanged += (_, _) => NotifyPropertyChanged(nameof(SelectionCounter));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
        SelectionMode = (SelectionMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? default;
    }

    /// Items
    public BindingList<StswSegmentTestModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private BindingList<StswSegmentTestModel> _items = new(Enumerable.Range(1, 15).Select(i => new StswSegmentTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 }).ToList());

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;

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

public class StswSegmentTestModel : StswObservableObject, IStswSelectionItem
{
    /// ID
    public int ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    private int _id;

    /// Name
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string? _name;

    /// IsSelected
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    private bool _isSelected;
}
