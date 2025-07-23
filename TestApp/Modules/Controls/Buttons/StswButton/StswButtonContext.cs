using System.Linq;

namespace TestApp;
public partial class StswButtonContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsDefault = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsDefault)))?.Value ?? default;
    }

    [StswCommand] void OnClick() => ClickCounter++;

    [StswObservableProperty] int _clickCounter;
    [StswObservableProperty] bool _isDefault;
}
