using System.Collections.ObjectModel;

namespace TestApp;

public class StswChartPieContext : ControlsContext
{
    /// Items
    private ObservableCollection<StswChartPieModel> items = new()
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
    public ObservableCollection<StswChartPieModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
}
