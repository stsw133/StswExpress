using System.Collections;
using System.Linq;

namespace TestApp;
public partial class StswDataPagerContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ItemsPerPage = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ItemsPerPage)))?.Value ?? default;
    }

    [StswObservableProperty] StswObservableCollection<StswDataGridTestModel> _items = new(Enumerable.Range(1, 1000).Select(i => new StswDataGridTestModel { Id = i, Name = "Row " + i }));
    [StswObservableProperty] IList? _itemsOnPage;
    [StswObservableProperty] int _itemsPerPage;
}
