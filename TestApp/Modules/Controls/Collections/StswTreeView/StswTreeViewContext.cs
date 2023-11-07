using System.ComponentModel;
using System.Linq;

namespace TestApp;

public class StswTreeViewContext : ControlsContext
{
    public StswTreeViewContext()
    {
        Items.ListChanged += (s, e) => NotifyPropertyChanged(nameof(SelectedItem));
    }

    #region Properties
    /// Items
    private BindingList<StswTreeViewTestModel> items = new()
    {
        new() { Name = "Option 1", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 1a" } } },
        new() { Name = "Option 2", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 2a" } } },
        new() { Name = "Option 3", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 3a", IsSelected = true } } },
        new() { Name = "Option 4", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 4a" } } },
        new() { Name = "Option 5", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 5a" } } },
        new() { Name = "Option 6", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 6a" } } },
        new() { Name = "Option 7", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 7a" } } },
        new() { Name = "Option 8", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 8a" } } },
        new() { Name = "Option 9", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 9a" } } },
        new() { Name = "Option 10", SubItems = new BindingList<StswTreeViewTestModel> { new() { Name = "Option 10a" } } }
    };
    public BindingList<StswTreeViewTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectedItem
    public object? SelectedItem => Items.AsEnumerable().FirstOrDefault(x => x.IsSelected);
    #endregion
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
    private object? subItems;
    public object? SubItems
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
