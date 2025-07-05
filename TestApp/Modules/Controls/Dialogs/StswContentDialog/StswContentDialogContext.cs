using System.Threading.Tasks;

namespace TestApp;
public partial class StswContentDialogContext : ControlsContext
{
    [StswAsyncCommand] async Task OpenContentDialog()
    {
        if (await StswContentDialog.Show(new ContractorsSingleDialogContext(), nameof(StswContentDialogView)) is string result)
            ContentDialogResult = result;
    }
    [StswAsyncCommand] async Task CloseContentDialog()
    {
        await Task.Run(() => StswContentDialog.Close(nameof(StswContentDialogView), true));
    }

    [StswObservableProperty] string? _contentDialogResult;
}
