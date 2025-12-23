using System.Linq;
using System.Threading.Tasks;

namespace TestApp.Wpf;
public partial class StswFileDialogContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        CloseOnBackdropClick = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(CloseOnBackdropClick)))?.Value ?? default;
    }

    [StswCommand] async Task OpenFileDialog()
    {
        var result = await StswFileDialog.Show(
            initialPath: "C:\\",
            filter: "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            identifier: nameof(StswFileDialogView));

        DialogResult = result?.ToString();
    }

    [StswObservableProperty] bool _closeOnBackdropClick;
    [StswObservableProperty] string? _dialogResult;
}
