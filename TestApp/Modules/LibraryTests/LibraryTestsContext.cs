using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class LibraryTestsContext : StswObservableObject
{
    /// AreControlsEnabled
    private bool areControlsEnabled = true;
    public bool AreControlsEnabled
    {
        get => areControlsEnabled;
        set => SetProperty(ref areControlsEnabled, value);
    }
    /// Number
    private double number = 0;
    public double Number
    {
        get => number;
        set => SetProperty(ref number, value);
    }
    /// Date
    private DateTime date = DateTime.Now;
    public DateTime Date
    {
        get => date;
        set => SetProperty(ref date, value);
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
        MessageBox.Show("TEST");
    }
}
