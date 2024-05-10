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
    public int ClickCounter
    {
        get => _clickCounter;
        set => SetProperty(ref _clickCounter, value);
    }
    private int _clickCounter;

    /// IsDefault
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }
    private bool _isDefault;
}
