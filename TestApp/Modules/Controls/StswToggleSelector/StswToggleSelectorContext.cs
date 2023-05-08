using System.Collections.Generic;

namespace TestApp;

public class StswToggleSelectorContext : StswObservableObject
{
    public List<StswComboItem?> ListTypes => new() { new() { Display = "Test1", Value = 1 }, new() { Display = "Test2", Value = 2 }, new() { Display = "Test3", Value = 3 }, null };
    public List<StswComboItem?> SelectedTypes => new() { new() { Display = "Test2", Value = 2 }, new() { Display = "Test3", Value = 3 }, null };
}
