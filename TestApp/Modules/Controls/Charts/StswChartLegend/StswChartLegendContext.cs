using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;

public class StswChartLegendContext : ControlsContext
{
    public StswCommand AddValueCommand => new(() => {
        Items.First(x => x.Name == "Option 9").Value += 20;
        Items = Items.OrderByDescending(x => x.Value).ToObservableCollection();
    });

    public override void SetDefaults()
    {
        base.SetDefaults();

        Columns = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Columns)))?.Value ?? default;
        Rows = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Rows)))?.Value ?? default;
        ShowDetails = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowDetails)))?.Value ?? default;
    }

    /// Columns
    public int Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }
    private int _columns;
    
    /// Items
    public ObservableCollection<StswChartElementModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    private ObservableCollection<StswChartElementModel> _items =
    [
        new() { Name = "Option 1", Value = 1000, Description = "The biggest (by default) source of value" },
        new() { Name = "Option 2", Value = 810 },
        new() { Name = "Option 3", Value = 640 },
        new() { Name = "Option 4", Value = 490 },
        new() { Name = "Option 5", Value = 360 },
        new() { Name = "Option 6", Value = 250 },
        new() { Name = "Option 7", Value = 160 },
        new() { Name = "Option 8", Value = 90 },
        new() { Name = "Option 9", Value = 40, Description = "Value of this source can be increased through button" },
        new() { Name = "Option 10", Value = 10, Description = "The smallest (by default) source of value" }
    ];

    /// Rows
    public int Rows
    {
        get => _rows;
        set => SetProperty(ref _rows, value);
    }
    private int _rows;

    /// ShowDetails
    public bool ShowDetails
    {
        get => _showDetails;
        set => SetProperty(ref _showDetails, value);
    }
    private bool _showDetails;
}
