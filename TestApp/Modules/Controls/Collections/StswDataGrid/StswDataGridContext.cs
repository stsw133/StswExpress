using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TestApp;

public class StswDataGridContext : ControlsContext
{
    public StswAsyncCommand OnScrollBottomCommand => new(OnScrollBottom);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        SpecialColumnVisibility = (StswSpecialColumnVisibility?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SpecialColumnVisibility)))?.Value ?? default;
    }

    #region Commands & methods
    /// OnScrollBottom
    public async Task OnScrollBottom()
    {
        await Task.Run(() =>
        {
            Thread.Sleep(1000);
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
                Items.AddRange(Enumerable.Range((Items.LastOrDefault()?.ID ?? 0) + 1, 15).Select(i => new StswDataGridTestModel { ID = i, Name = "Row " + i, ShowDetails = i % 3 == 0 ? null : false }), StswItemState.Unchanged);
            });
        });
    }
    #endregion

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Items
    private StswBindingList<StswDataGridTestModel> items = new(Enumerable.Range(1, 15).Select(i => new StswDataGridTestModel { ID = i, Name = "Row " + i, ShowDetails = i % 3 == 0 ? null : false }));
    public StswBindingList<StswDataGridTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// SpecialColumnVisibility
    private StswSpecialColumnVisibility specialColumnVisibility;
    public StswSpecialColumnVisibility SpecialColumnVisibility
    {
        get => specialColumnVisibility;
        set => SetProperty(ref specialColumnVisibility, value);
    }
}

public class StswDataGridTestModel : StswObservableObject, IStswCollectionItem, IStswSelectionItem
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
