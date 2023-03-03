using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace TestApp;

public class StswTextBoxContext : StswObservableObject
{
    private string text = string.Empty;
    public string Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }

    /// ...
    public ICommand ClearCommand { get; set; }
    public ICommand SearchCommand { get; set; }

    public StswTextBoxContext()
    {
        ClearCommand = new StswRelayCommand(Clear);
        SearchCommand = new StswRelayCommand(Search);
    }

    /// ClearCommand
    private void Clear()
    {
        Text = string.Empty;
    }
    /// SearchCommand
    private void Search()
    {
        MessageBox.Show(Text);
    }
}
