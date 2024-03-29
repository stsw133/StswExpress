using System.Linq;

namespace TestApp;

public class StswLoadingMaskContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
    }

    /// IsBusy
    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }
}
