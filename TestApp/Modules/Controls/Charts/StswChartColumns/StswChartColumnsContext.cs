using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;

public class StswChartColumnsContext : ControlsContext
{
    public StswCommand AddValueCommand => new(() => {
        Items.First(x => x.Name == "Option 9").Value += 20;
        Items = Items.OrderByDescending(x => x.Value).ToObservableCollection();
    });

    /// Items
    private ObservableCollection<StswChartElementModel> items = new()
    {
        /*
        new() { Name = "Option 1", Elements = new() { new() { Name = "A", Value = 1000 }, new() { Name = "B", Value = 1210 } } },
        new() { Name = "Option 2", Elements = new() { new() { Name = "A", Value = 810 }, new() { Name = "B", Value = 1440 } } },
        new() { Name = "Option 3", Elements = new() { new() { Name = "A", Value = 640 }, new() { Name = "B", Value = 1690 } } },
        new() { Name = "Option 4", Elements = new() { new() { Name = "A", Value = 490 }, new() { Name = "B", Value = 1960 } } },
        new() { Name = "Option 5", Elements = new() { new() { Name = "A", Value = 360 }, new() { Name = "B", Value = 2250 } } },
        new() { Name = "Option 6", Elements = new() { new() { Name = "A", Value = 250 }, new() { Name = "B", Value = 2560 } } },
        new() { Name = "Option 7", Elements = new() { new() { Name = "A", Value = 160 }, new() { Name = "B", Value = 2890 } } },
        new() { Name = "Option 8", Elements = new() { new() { Name = "A", Value = 90 }, new() { Name = "B", Value = 3240 } } },
        new() { Name = "Option 9", Elements = new() { new() { Name = "A", Value = 40 }, new() { Name = "B", Value = 3610 } } },
        new() { Name = "Option 10", Elements = new() { new() { Name = "A", Value = 10 }, new() { Name = "B", Value = 4000 } } }
        */
    };
    public ObservableCollection<StswChartElementModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
}
