using System.Linq;

namespace TestApp.Wpf;
public partial class StswBarcodeContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        CodeType = (StswBarcodeType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(CodeType)))?.Value ?? default;
        Value = (string?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Value)))?.Value ?? default;
    }

    [StswObservableProperty] StswBarcodeType _codeType;
    [StswObservableProperty] string? _value;
}
