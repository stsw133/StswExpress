using System.Collections.Generic;
using System.Data;

namespace TestApp;

public class StswDataGridContext : ControlsContext
{
    #region Properties
    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Items
    private StswCollection<StswDataGridTestModel> items = new(
            new List<StswDataGridTestModel>()
            {
                new() { ID = 1, Name = "First row", ShowDetails = false },
                new() { ID = 2, Name = "Second row" },
                new() { ID = 3, Name = "Third row" },
                new() { ID = 4, Name = "Fourth row", ShowDetails = false },
                new() { ID = 5, Name = "Fifth row" }
            }
        );
    public StswCollection<StswDataGridTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SpecialColumnVisibility
    private StswSpecialColumnVisibility specialColumnVisibility = StswSpecialColumnVisibility.All;
    public StswSpecialColumnVisibility SpecialColumnVisibility
    {
        get => specialColumnVisibility;
        set => SetProperty(ref specialColumnVisibility, value);
    }
    #endregion
}

public class StswDataGridTestModel : StswObservableObject, IStswCollectionItem
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

    /// ErrorMessage
    private string? errorMessage;
    public string? ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    /// ItemState
    private DataRowState itemState;
    public DataRowState ItemState
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
}
