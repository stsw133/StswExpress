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
    }

    #region Commands & methods
    /// OnScrollBottom
    public async Task OnScrollBottom()
    {
        IsBusy = true;
        await Task.Run(() =>
        {
            Thread.Sleep(1000);
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
                Items.AddRange(Enumerable.Range((Items.LastOrDefault()?.ID ?? 0) + 1, 15).Select(i => new StswDataGridTestModel { ID = i, Name = "Row " + i, ShowDetails = i % 3 == 0 ? null : false }), StswItemState.Unchanged);
            });
        });
        IsBusy = false;
    }
    #endregion

    /// IsBusy
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// Items
    public StswBindingList<StswDataGridTestModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private StswBindingList<StswDataGridTestModel> _items = new(Enumerable.Range(1, 15).Select(i => new StswDataGridTestModel { ID = i, Name = "Row " + i, ShowDetails = i % 3 == 0 ? null : false }));
}

public class StswDataGridTestModel : StswObservableObject, IStswCollectionItem, IStswSelectionItem
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

    /// ItemState
    public StswItemState ItemState
    {
        get => _itemState;
        set => SetProperty(ref _itemState, value);
    }
    private StswItemState _itemState;

    /// ShowDetails
    public bool? ShowDetails
    {
        get => _showDetails;
        set => SetProperty(ref _showDetails, value);
    }
    private bool? _showDetails;

    /// IsSelected
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    private bool _isSelected;
}
