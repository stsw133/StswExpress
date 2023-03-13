using System.Data;

namespace StswExpress;

public class StswCollectionItem : StswObservableObject
{
    /// ItemState
    private DataRowState itemState = DataRowState.Unchanged;
    public DataRowState ItemState
    {
        get => itemState;
        set => SetProperty(ref itemState, value);
    }

    /// ShowDetails
    public bool ShowDetails { get; set; }
}
