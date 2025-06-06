﻿using System.Threading.Tasks;

namespace TestApp;

public class StswContentDialogContext : ControlsContext
{
    public StswAsyncCommand OpenContentDialogCommand => new(OpenContentDialog);
    public StswAsyncCommand CloseContentDialogCommand => new(CloseContentDialog);

    #region Events & methods
    /// Command: open content dialog
    private async Task OpenContentDialog()
    {
        if (await StswContentDialog.Show(new ContractorsSingleDialogContext(), nameof(StswContentDialogView)) is string result)
            ContentDialogResult = result;
    }

    /// Command: close content dialog
    private async Task CloseContentDialog()
    {
        await Task.Run(() => StswContentDialog.Close(nameof(StswContentDialogView), true));
    }
    #endregion

    /// ContentDialogResult
    public string? ContentDialogResult
    {
        get => _contentDialogResult;
        set => SetProperty(ref _contentDialogResult, value);
    }
    private string? _contentDialogResult;
}
