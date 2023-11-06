using System.Linq;
using System.Windows.Controls;

namespace TestApp;

public class StswListBoxContext : ControlsContext
{
    public StswListBoxContext()
    {
        Items.ListChanged += (s, e) => NotifyPropertyChanged(nameof(SelectionCounter));
    }

    #region Properties
    /// Items
    private StswBindingList<StswListBoxTestModel> items = new()
    {
        new() { Name = "Option 1", IsSelected = true },
        new() { Name = "Option 2", IsSelected = false },
        new() { Name = "Option 3", IsSelected = false },
        new() { Name = "Option 4", IsSelected = true },
        new() { Name = "Option 5", IsSelected = false },
        new() { Name = "Option 6", IsSelected = true },
        new() { Name = "Option 7", IsSelected = false },
        new() { Name = "Option 8", IsSelected = false },
        new() { Name = "Option 9", IsSelected = true },
        new() { Name = "Option 10", IsSelected = false }
    };
    public StswBindingList<StswListBoxTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SelectionCounter
    public int SelectionCounter => Items.AsEnumerable().Count(x => x.IsSelected);

    /// SelectionMode
    private SelectionMode selectionMode = SelectionMode.Multiple;
    public SelectionMode SelectionMode
    {
        get => selectionMode;
        set => SetProperty(ref selectionMode, value);
    }
    #endregion
}

public class StswListBoxTestModel : StswObservableObject, IStswCollectionItem, IStswSelectionItem
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
