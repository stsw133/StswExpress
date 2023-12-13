using System.Collections.ObjectModel;
using System.Windows.Media;

namespace TestApp;

public class StswChartPieContext : ControlsContext
{
    #region Properties
    /// Items
    private ObservableCollection<StswChartModel> items = new()
    {
        new() { Name = "Option 1", Value = 100, Brush = new SolidColorBrush(Colors.Red) },
        new() { Name = "Option 2", Value = 81, Brush = new SolidColorBrush(Colors.Blue) },
        new() { Name = "Option 3", Value = 64, Brush = new SolidColorBrush(Colors.Green) },
        new() { Name = "Option 4", Value = 49, Brush = new SolidColorBrush(Colors.Yellow) },
        new() { Name = "Option 5", Value = 36, Brush = new SolidColorBrush(Colors.Magenta) },
        new() { Name = "Option 6", Value = 25, Brush = new SolidColorBrush(Colors.Cyan) },
        new() { Name = "Option 7", Value = 16, Brush = new SolidColorBrush(Colors.Orange) },
        new() { Name = "Option 8", Value = 9, Brush = new SolidColorBrush(Colors.Brown) },
        new() { Name = "Option 9", Value = 4, Brush = new SolidColorBrush(Colors.Teal) },
        new() { Name = "Option 10", Value = 1, Brush = new SolidColorBrush(Colors.Gray) }
    };
    public ObservableCollection<StswChartModel> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    #endregion
}
