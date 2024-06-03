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
    public string? ContentDialogResult
    {
        get => _contentDialogResult;
        set => SetProperty(ref _contentDialogResult, value);
    }
    private string? _contentDialogResult;

    /// IsOpen
    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }
    private bool _isOpen;
}
