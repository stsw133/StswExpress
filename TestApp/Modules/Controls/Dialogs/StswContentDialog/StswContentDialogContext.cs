using System.Windows.Input;

namespace TestApp;

public class StswContentDialogContext : ControlsContext
{
    public ICommand OpenContentDialogCommand { get; set; }

    public StswContentDialogContext()
    {
        OpenContentDialogCommand = new StswCommand(OpenContentDialog);
    }

    #region Events and methods
    /// Command: open content dialog
    private void OpenContentDialog() => IsOpen = true;
    #endregion

    #region Properties
    /// ContentDialogResult
    private string? contentDialogResult;
    public string? ContentDialogResult
    {
        get => contentDialogResult;
        set => SetProperty(ref contentDialogResult, value);
    }

    /// IsOpen
    private bool isOpen;
    public bool IsOpen
    {
        get => isOpen;
        set => SetProperty(ref isOpen, value);
    }
    #endregion
}
