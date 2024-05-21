using System.Collections;
using System.Linq;

namespace TestApp;

public class StswDataPagerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ItemsPerPage = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsPerPage)))?.Value ?? default;
    }

    /// Items
    public StswBindingList<StswDataGridTestModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private StswBindingList<StswDataGridTestModel> _items = new(Enumerable.Range(1, 1000).Select(i => new StswDataGridTestModel { ID = i, Name = "Row " + i, ShowDetails = i % 3 == 0 ? null : false }).ToList());

    /// ItemsOnPage
    public IList? ItemsOnPage
    {
        get => _itemsOnPage;
        set => SetProperty(ref _itemsOnPage, value);
    }
    private IList? _itemsOnPage;

    /// ItemsPerPage
    public int ItemsPerPage
    {
        get => _itemsPerPage;
        set => SetProperty(ref _itemsPerPage, value);
    }
    private int _itemsPerPage;
}
