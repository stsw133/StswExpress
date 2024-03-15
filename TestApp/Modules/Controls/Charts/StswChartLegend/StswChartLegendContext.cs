using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;

public class StswChartLegendContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Columns = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Columns)))?.Value ?? default;
        Rows = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Rows)))?.Value ?? default;
        ShowDetails = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ShowDetails)))?.Value ?? default;
    }

    /// Columns
    private int columns;
    public int Columns
    {
        get => columns;
        set => SetProperty(ref columns, value);
    }
    
    /// Items
    private ObservableCollection<StswChartLegendModel> items = new()
    {
        new() { Name = "Option 1", Value = 100 },
        new() { Name = "Option 2", Value = 81 },
        new() { Name = "Option 3", Value = 64 },
        new() { Name = "Option 4", Value = 49 },
        new() { Name = "Option 5", Value = 36 },
        new() { Name = "Option 6", Value = 25 },
        new() { Name = "Option 7", Value = 16 },
        new() { Name = "Option 8", Value = 9 },
        new() { Name = "Option 9", Value = 4 },
        new() { Name = "Option 10", Value = 1 }
    };
    public ObservableCollection<StswChartLegendModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }

    /// Rows
    private int rows;
    public int Rows
    {
        get => rows;
        set => SetProperty(ref rows, value);
    }

    /// ShowDetails
    private bool showDetails;
    public bool ShowDetails
    {
        get => showDetails;
        set => SetProperty(ref showDetails, value);
    }
}
