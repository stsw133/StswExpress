using System.Collections.Generic;
using System.Windows.Input;

namespace TestApp.Modules.LibraryTests;

public class LibraryTestsContext : StswObservableObject
{
    /// Number
    private int number = 0;
    public int Number
    {
        get => number;
        set => SetProperty(ref number, value);
    }

    /// ComboLists
    public List<string?> ListTypes => new List<string?>() { "Test1", "Test2", "Test3", null };
    public List<string?> SelectedTypes => new List<string?>() { "Test2", "Test3", null };

    /// ...
    public ICommand SearchCommand { get; set; }

    public LibraryTestsContext()
    {
        SearchCommand = new StswRelayCommand(Search);
    }

    /// SearchCommand
    private void Search()
    {
        if (Number < 100)
            Number++;
    }
}
