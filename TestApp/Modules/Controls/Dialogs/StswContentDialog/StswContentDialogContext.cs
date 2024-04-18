using System.Threading.Tasks;

namespace TestApp;

public class StswContentDialogContext : ControlsContext
{
    public StswAsyncCommand OpenContentDialogCommand => new(OpenContentDialog);
    public StswAsyncCommand CloseContentDialogCommand => new(CloseContentDialog);

    #region Events & methods
    /// Command: open content dialog
    private async Task OpenContentDialog()
    {
        //if (await StswContentDialog.Show(new ContractorsContext(), nameof(StswContentDialogView)) is string result)
        //    ContentDialogResult = result;

        //StswContentDialog.Show(new ContractorsContext(), StswApp.StswWindow);
        await Task.Run(() => IsOpen = true);
    }

    /// Command: close content dialog
    private async Task CloseContentDialog()
    {
        //StswContentDialog.Close(nameof(StswContentDialogView), true);
        await Task.Run(() => IsOpen = false);
    }
    #endregion

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
}
