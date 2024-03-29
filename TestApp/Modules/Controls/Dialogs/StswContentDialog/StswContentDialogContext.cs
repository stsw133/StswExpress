﻿using System.Threading.Tasks;

namespace TestApp;

public class StswContentDialogContext : ControlsContext
{
    public StswAsyncCommand OpenContentDialogCommand => new(OpenContentDialog);

    #region Events & methods
    /// Command: open content dialog
    private async Task OpenContentDialog()
    {
        //if (await StswContentDialog.Show(new ContractorsContext(), "TEST") is string result)
        //    ContentDialogResult = result;

        //StswContentDialog.Show(new ContractorsContext(), StswApp.StswWindow);

        await Task.Run(() => IsOpen = true);

        //StswContentDialog.Close("TEST", true);
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
