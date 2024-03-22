using System.Linq;

namespace TestApp;

public class StswButtonContext : ControlsContext
{
    public StswCommand OnClickCommand => new(() => ClickCounter++);

    public override void SetDefaults()
    {
        base.SetDefaults();

        IsDefault = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsDefault)))?.Value ?? default;
    }

    /// ClickCounter
    private int clickCounter;
    public int ClickCounter
    {
        get => clickCounter;
        set => SetProperty(ref clickCounter, value);
    }

    /// IsDefault
    private bool isDefault;
    public bool IsDefault
    {
        get => isDefault;
        set => SetProperty(ref isDefault, value);
    }
}
