using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswListBoxContext : ControlsContext
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

    [StswObservableProperty] BindingList<StswListBoxTestModel> _items = new([.. Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 })]);
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] SelectionMode _selectionMode;
}

public partial class StswListBoxTestModel : StswObservableObject, IStswSelectionItem
{
    [StswObservableProperty] int _id;
    [StswObservableProperty] string? _name;
    [StswObservableProperty] bool _isSelected;
}
