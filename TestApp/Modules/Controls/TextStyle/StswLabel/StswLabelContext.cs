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
    public bool IsTruncationAllowed
    {
        get => _isTruncationAllowed;
        set => SetProperty(ref _isTruncationAllowed, value);
    }
    private bool _isTruncationAllowed;
}
