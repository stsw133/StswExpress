using System.Linq;

namespace TestApp;

public class StswLabelContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        AutoTruncation = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(AutoTruncation)))?.Value ?? default;
        IsContentTruncated = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentTruncated)))?.Value ?? default;
    }

    /// AutoTruncation
    public bool AutoTruncation
    {
        get => _autoTruncation;
        set => SetProperty(ref _autoTruncation, value);
    }
    private bool _autoTruncation;

    /// IsContentTruncated
    public bool IsContentTruncated
    {
        get => _isContentTruncated;
        set => SetProperty(ref _isContentTruncated, value);
    }
    private bool _isContentTruncated;
}
