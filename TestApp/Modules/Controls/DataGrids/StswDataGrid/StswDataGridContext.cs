using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TestApp;
public partial class StswDataGridContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
        ScrollToItemBehavior = (StswScrollToItemBehavior?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ScrollToItemBehavior)))?.Value ?? default;
        SelectionUnit = (DataGridSelectionUnit?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(SelectionUnit)))?.Value ?? default;
    }

    [StswCommand] async Task OnScrollBottom()
    {
        IsBusy = true;
        await Task.Run(() =>
        {
            Thread.Sleep(1000);
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                () => Items.AddRange(
                    Enumerable.Range((Items.LastOrDefault()?.Id ?? 0) + 1, 15).Select(i => new StswDataGridTestModel { Id = i, Name = "Row " + i, ShowDetails = i % 3 == 0 ? null : false })
                    ));
        });
        IsBusy = false;
    }

    [StswObservableProperty] bool _isBusy;
    [StswObservableProperty] bool _isReadOnly;
    [StswObservableProperty] StswObservableCollection<StswDataGridTestModel> _items = new(Enumerable.Range(1, 15).Select(i => new StswDataGridTestModel
    {
        Id = i,
        Name = "Row " + i,
        ShowDetails = i % 3 == 0 ? null : false
    }));
    [StswObservableProperty] StswScrollToItemBehavior _scrollToItemBehavior;
    [StswObservableProperty] DataGridSelectionUnit _selectionUnit;
}

public partial class StswDataGridTestModel : StswObservableObject, IStswCollectionItem, IStswSelectionItem
{
    [StswObservableProperty] int _id;
    [StswObservableProperty] string? _name;
    [StswObservableProperty] StswItemState _itemState;
    [StswObservableProperty] bool? _showDetails;
    [StswObservableProperty] bool _isSelected;
}
