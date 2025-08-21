using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswDragBoxContext : ControlsContext
{
    public StswDragBoxContext()
    {
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(SelectionCounter));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        ScrollToItemBehavior = (StswScrollToItemBehavior?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ScrollToItemBehavior)))?.Value ?? default;
        SelectionMode = (SelectionMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? default;
    }

    [StswObservableProperty] ObservableCollection<StswListBoxTestModel> _items = new([.. Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 })]);
    [StswObservableProperty] ObservableCollection<StswListBoxTestModel> _items2 = new([.. Enumerable.Range(16, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 })]);
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] StswScrollToItemBehavior _scrollToItemBehavior;
    [StswObservableProperty] SelectionMode _selectionMode;
}
