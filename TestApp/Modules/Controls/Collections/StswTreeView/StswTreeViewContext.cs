using System;
using System.ComponentModel;
using System.Linq;

namespace TestApp;

public class StswTreeViewContext : ControlsContext
{
    public StswTreeViewContext()
    {
        Items.ListChanged += (s, e) => NotifyPropertyChanged(nameof(SelectedItem));
    }

    /// Items
    private BindingList<StswTreeViewTestModel> items = new(Enumerable.Range(1, 15).Select(i => new StswTreeViewTestModel
    {
        Name = "Option " + i,
        SubItems = new(Enumerable.Range(97, 5).Select(j => new StswTreeViewTestModel { Name = "Option " + i + (char)j, IsSelected = new Random().Next(2) == 0 } ).ToList())
    }).ToList());
    public BindingList<StswTreeViewTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectedItem
    public object? SelectedItem => Items.AsEnumerable().FirstOrDefault(x => x.IsSelected)?.Name ?? "none or one of sub-items";
    //public object? SelectedItem => Items.SelectMany(item => new[] { item }.Concat(item.SubItems?.SelectMany(subItem => new[] { subItem }.Concat(subItem.SubItems ?? Enumerable.Empty<StswTreeViewTestModel>())) ?? Enumerable.Empty<StswTreeViewTestModel>())).FirstOrDefault(item => item.IsSelected);
}

public class StswTreeViewTestModel : StswObservableObject, IStswSelectionItem
{
    /// ID
    private int id;
    public int ID
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    /// Name
    private string? name;
    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    /// SubItems
    private BindingList<StswTreeViewTestModel>? subItems;
    public BindingList<StswTreeViewTestModel>? SubItems
    {
        get => subItems;
        set => SetProperty(ref subItems, value);
    }

    /// IsSelected
    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }
}
