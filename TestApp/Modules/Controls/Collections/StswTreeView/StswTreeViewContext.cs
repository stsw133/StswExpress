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
    private StswBindingList<StswTreeViewTestModel> items = new()
    {
        new() { Name = "Option 1", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 1a" } } },
        new() { Name = "Option 2", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 2a" } } },
        new() { Name = "Option 3", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 3a", IsSelected = true } } },
        new() { Name = "Option 4", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 4a" } } },
        new() { Name = "Option 5", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 5a" } } },
        new() { Name = "Option 6", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 6a" } } },
        new() { Name = "Option 7", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 7a" } } },
        new() { Name = "Option 8", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 8a" } } },
        new() { Name = "Option 9", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 9a" } } },
        new() { Name = "Option 10", SubItems = new StswBindingList<StswTreeViewTestModel> { new() { Name = "Option 10a" } } }
    };
    public StswBindingList<StswTreeViewTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectedItem
    public object? SelectedItem => Items.AsEnumerable().FirstOrDefault(x => x.IsSelected);
    #endregion
}

public class StswTreeViewTestModel : StswObservableObject, IStswCollectionItem, IStswSelectionItem
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

    /// ItemMessage
    private string? itemMessage;
    public string? ItemMessage
    {
        get => itemMessage;
        set => SetProperty(ref itemMessage, value);
    }

    /// ItemState
    private StswItemState itemState;
    public StswItemState ItemState
    {
        get => itemState;
        set => SetProperty(ref itemState, value);
    }

    /// ShowDetails
    private bool? showDetails;
    public bool? ShowDetails
    {
        get => showDetails;
        set => SetProperty(ref showDetails, value);
    }

    /// IsSelected
    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }
}
