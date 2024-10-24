using System;
using System.ComponentModel;
using System.Linq;

namespace TestApp;

public class StswTreeViewContext : ControlsContext
{
    public StswTreeViewContext()
    {
        Items.ListChanged += (_, _) => OnPropertyChanged(nameof(SelectedItem));
    }

    /// Items
    public BindingList<StswTreeViewTestModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private BindingList<StswTreeViewTestModel> _items = new(Enumerable.Range(1, 15).Select(i => new StswTreeViewTestModel
    {
        Name = "Option " + i,
        SubItems = new(Enumerable.Range(97, 5).Select(j => new StswTreeViewTestModel { Name = "Option " + i + (char)j, IsSelected = new Random().Next(2) == 0 } ).ToList())
    }).ToList());

    /// SelectedItem
    public object? SelectedItem => Items.AsEnumerable().FirstOrDefault(x => x.IsSelected)?.Name ?? "none or one of sub-items";
    //public object? SelectedItem => Items.SelectMany(item => new[] { item }.Concat(item.SubItems?.SelectMany(subItem => new[] { subItem }.Concat(subItem.SubItems ?? Enumerable.Empty<StswTreeViewTestModel>())) ?? Enumerable.Empty<StswTreeViewTestModel>())).FirstOrDefault(item => item.IsSelected);
}

public class StswTreeViewTestModel : StswObservableObject, IStswSelectionItem
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

    /// SubItems
    public BindingList<StswTreeViewTestModel>? SubItems
    {
        get => _subItems;
        set => SetProperty(ref _subItems, value);
    }
    private BindingList<StswTreeViewTestModel>? _subItems;

    /// IsSelected
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    private bool _isSelected;
}
