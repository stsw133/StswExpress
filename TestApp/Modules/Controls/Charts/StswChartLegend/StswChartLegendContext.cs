using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;
public partial class StswChartLegendContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        VerticalAlignment = System.Windows.VerticalAlignment.Top;

        Columns = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Columns)))?.Value ?? default;
        Rows = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Rows)))?.Value ?? default;
        ShowDetails = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowDetails)))?.Value ?? default;
    }

    [StswCommand] void AddValue()
    {
        Items.First(x => x.Title == "Option 9").Value += 20;
        Items = [.. Items.OrderByDescending(x => x.Value)];
    }

    [StswObservableProperty] int _columns;
    [StswObservableProperty] ObservableCollection<StswChartItemTestModel> _items =
    [
        new() { Title = "Option 1", Value = 1000, Description = "The biggest (by default) source of value" },
        new() { Title = "Option 2", Value = 810 },
        new() { Title = "Option 3", Value = 640 },
        new() { Title = "Option 4", Value = 490 },
        new() { Title = "Option 5", Value = 360 },
        new() { Title = "Option 6", Value = 250 },
        new() { Title = "Option 7", Value = 160 },
        new() { Title = "Option 8", Value = 90 },
        new() { Title = "Option 9", Value = 40, Description = "Value of this source can be increased through button" },
        new() { Title = "Option 10", Value = 10, Description = "The smallest (by default) source of value" }
    ];
    [StswObservableProperty] int _rows;
    [StswObservableProperty] bool _showDetails;
}
