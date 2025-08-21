using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace TestApp;
public partial class StswListViewContext : ControlsContext
{
    public StswListViewContext()
    {
        Items.ListChanged += (_, _) => OnPropertyChanged(nameof(SelectionCounter));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        ScrollToItemBehavior = (StswScrollToItemBehavior?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ScrollToItemBehavior)))?.Value ?? default;
        SelectionMode = (SelectionMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionMode)))?.Value ?? SelectionMode.Multiple; //default;
    }

    [StswObservableProperty] BindingList<StswListBoxTestModel> _items = new([.. Enumerable.Range(1, 15).Select(i => new StswListBoxTestModel { Name = "Option " + i, IsSelected = new Random().Next(2) == 0 })]);
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] StswScrollToItemBehavior _scrollToItemBehavior;
    [StswObservableProperty] SelectionMode _selectionMode;
}
