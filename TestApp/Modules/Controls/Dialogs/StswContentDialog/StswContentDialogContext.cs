using System.Threading.Tasks;

namespace TestApp;
public partial class StswContentDialogContext : ControlsContext
{
    [StswCommand] async Task OpenContentDialog()
    {
        if (await StswContentDialog.Show(new ContractorsSingleDialogContext(), nameof(StswContentDialogView)) is string result)
            ContentDialogResult = result;
    }
    [StswCommand] async Task CloseContentDialog()
    {
        await Task.Run(() => StswContentDialog.Close(nameof(StswContentDialogView), true));
    }

    [StswObservableProperty] string? _contentDialogResult;
}
