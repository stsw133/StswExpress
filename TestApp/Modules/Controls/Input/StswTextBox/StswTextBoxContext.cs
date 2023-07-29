using System.Windows.Input;
using System;

namespace TestApp;

public class StswTextBoxContext : ControlsContext
{
    public ICommand RandomizeCommand { get; set; }

    public StswTextBoxContext()
    {
        RandomizeCommand = new StswCommand(Randomize);
    }

    #region Events and methods
    /// Command: randomize
    private void Randomize() => Text = Guid.NewGuid().ToString();
    #endregion

    #region Properties
    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
    }

    /// IsReadOnly
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Text
    private string text = string.Empty;
    public string Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }
    #endregion
}
