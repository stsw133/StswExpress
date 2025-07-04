using System;
using System.ComponentModel;
using System.Linq;

namespace TestApp;
public partial class StswTreeViewContext : ControlsContext
{
    public StswTreeViewContext()
    {
        Items.ListChanged += (_, _) => OnPropertyChanged(nameof(SelectedItem));
    }
    
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    [StswObservableProperty]
    BindingList<StswTreeViewTestModel> _items = new([.. Enumerable.Range(1, 15).Select(i => new StswTreeViewTestModel
    {
        Name = "Option " + i,
        SubItems = new([.. Enumerable.Range(97, 5).Select(j => new StswTreeViewTestModel { Name = "Option " + i + (char)j, IsSelected = new Random().Next(2) == 0 } )])
    })]);
    public object? SelectedItem => Items.AsEnumerable().FirstOrDefault(x => x.IsSelected)?.Name ?? "none or one of sub-items";
    //public object? SelectedItem => Items.SelectMany(item => new[] { item }.Concat(item.SubItems?.SelectMany(subItem => new[] { subItem }.Concat(subItem.SubItems ?? Enumerable.Empty<StswTreeViewTestModel>())) ?? Enumerable.Empty<StswTreeViewTestModel>())).FirstOrDefault(item => item.IsSelected);
    [StswObservableProperty] bool _isReadOnly;
}

public partial class StswTreeViewTestModel : StswObservableObject, IStswSelectionItem
{
    [StswObservableProperty] int _id;
    [StswObservableProperty] string? _name;
    [StswObservableProperty] BindingList<StswTreeViewTestModel>? _subItems;
    [StswObservableProperty] bool _isSelected;
}
