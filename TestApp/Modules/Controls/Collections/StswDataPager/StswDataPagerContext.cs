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
    private StswBindingList<StswDataGridTestModel> items = new(Enumerable.Range(1, 1000).Select(i => new StswDataGridTestModel { ID = i, Name = "Row " + i, ShowDetails = i % 3 == 0 ? null : false }).ToList());
    public StswBindingList<StswDataGridTestModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// ItemsOnPage
    private IList? itemsOnPage;
    public IList? ItemsOnPage
    {
        get => itemsOnPage;
        set => SetProperty(ref itemsOnPage, value);
    }

    /// ItemsPerPage
    private int itemsPerPage;
    public int ItemsPerPage
    {
        get => itemsPerPage;
        set => SetProperty(ref itemsPerPage, value);
    }
}
