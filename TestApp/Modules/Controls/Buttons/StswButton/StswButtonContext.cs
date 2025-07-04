using System.Linq;

namespace TestApp;
public partial class StswButtonContext : ControlsContext
{
    public StswCommand OnClickCommand => new(() => ClickCounter++);
    
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsDefault = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsDefault)))?.Value ?? default;
    }

    [StswObservableProperty] int _clickCounter;
    [StswObservableProperty] bool _isDefault;
}
