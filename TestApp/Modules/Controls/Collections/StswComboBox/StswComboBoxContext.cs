using System.Collections.Generic;

namespace TestApp;

public class StswComboBoxContext : StswObservableObject
{
    public List<string?> ListTypes => new List<string?>() { "Format1", "Test2", "Type3", null };
    public List<string?> SelectedTypes => new List<string?>() { "Test2", "Type3", null };
}
