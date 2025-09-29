using System.Linq;
using System.Threading.Tasks;

namespace TestApp;
public partial class StswContentDialogContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        CloseOnBackdropClick = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(CloseOnBackdropClick)))?.Value ?? default;
    }

    [StswCommand] async Task OpenContentDialog()
    {
        if (await StswContentDialog.Show(new ContractorsSingleDialogContext(), nameof(StswContentDialogView)) is string result)
            ContentDialogResult = result;
    }
    [StswCommand] async Task CloseContentDialog()
    {
        await Task.Run(() => StswContentDialog.Close(nameof(StswContentDialogView), true));
    }

    [StswObservableProperty] bool _closeOnBackdropClick;
    [StswObservableProperty] string? _contentDialogResult;
}
