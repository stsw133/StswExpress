using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswSegmentContext : ControlsContext
{
    public StswSegmentContext()
    {
        Items.ListChanged += (_, _) => OnPropertyChanged(nameof(SelectionCounter));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
        ScrollToItemBehavior = (StswScrollToItemBehavior?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ScrollToItemBehavior)))?.Value ?? default;
        SelectionMode = (SelectionMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? default;
    }

    [StswObservableProperty] BindingList<StswSegmentTestModel> _items = new([.. Enumerable.Range(1, 15).Select(i => new StswSegmentTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 })]);
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] Orientation _orientation;
    [StswObservableProperty] StswScrollToItemBehavior _scrollToItemBehavior;
    [StswObservableProperty] SelectionMode _selectionMode;
}

public partial class StswSegmentTestModel : StswObservableObject, IStswSelectionItem
{
    [StswObservableProperty] int _id;
    [StswObservableProperty] string? _name;
    [StswObservableProperty] bool _isSelected;
}
