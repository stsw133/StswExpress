using System.Collections.Generic;

namespace TestApp.Modules.LibraryTests;

public class LibraryTestsContext : StswObservableObject
{
    /// ComboLists
    public List<string?> ListTypes => new List<string?>() { "Test1", "Test2", "Test3", null };
    public List<string?> SelectedTypes => new List<string?>() { "Test2", "Test3", null };
}
