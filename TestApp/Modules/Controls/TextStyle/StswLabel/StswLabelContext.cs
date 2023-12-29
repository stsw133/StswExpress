using System.Linq;

namespace TestApp;

public class StswLabelContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsTruncationAllowed = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsTruncationAllowed)))?.Value ?? default;
    }

    /// IsTruncationAllowed
    private bool isTruncationAllowed;
    public bool IsTruncationAllowed
    {
        get => isTruncationAllowed;
        set => SetProperty(ref isTruncationAllowed, value);
    }
}
