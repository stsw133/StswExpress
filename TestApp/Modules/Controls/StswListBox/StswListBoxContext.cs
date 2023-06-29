using System.Collections.Generic;

namespace TestApp;

public class StswListBoxContext : StswObservableObject
{
    public List<string?> Items => new List<string?>() { "Format1", "Test2", "Type3", null };
}
