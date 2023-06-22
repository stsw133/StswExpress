using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;

public class StswComboViewContext : StswObservableObject
{
    public StswComboViewContext()
    {
        SelectedTypes = ListTypes.Where(x => x?.Value?.In(1, 2) == true).ToObservableCollection();
    }

    /// ListTypes
    private ObservableCollection<StswComboItem?> listTypes = new()
    {
        new() { Display = "Test1", Value = 1 },
        new() { Display = "Test2", Value = 2 },
        new() { Display = "Test3", Value = 3 },
        null
    };
    public ObservableCollection<StswComboItem?> ListTypes
    {
        get => listTypes;
        set => SetProperty(ref listTypes, value);
    }

    /// SelectedTypes
    private ObservableCollection<StswComboItem?> selectedTypes = new();
    public ObservableCollection<StswComboItem?> SelectedTypes
    {
        get => selectedTypes;
        set => SetProperty(ref selectedTypes, value);
    }
}
