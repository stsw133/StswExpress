using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswListBoxContext : ControlsContext
{
    public StswListBoxContext()
    {
        Items.ListChanged += (_, _) => OnPropertyChanged(nameof(SelectionCounter));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
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

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// SelectionMode
    public SelectionMode SelectionMode
    {
        get => _selectionMode;
        set => SetProperty(ref _selectionMode, value);
    }
    private SelectionMode _selectionMode;
}

public class StswListBoxTestModel : StswObservableObject, IStswSelectionItem
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
