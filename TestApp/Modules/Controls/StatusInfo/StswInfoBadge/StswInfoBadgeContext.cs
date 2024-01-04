using System.Linq;

namespace TestApp;

public class StswInfoBadgeContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Format = (StswInfoFormat?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Format)))?.Value ?? default;
        Limit = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Limit)))?.Value ?? default;
        Type = (StswInfoType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    /// Format
    private StswInfoFormat format;
    public StswInfoFormat Format
    {
        get => format;
        set => SetProperty(ref format, value);
    }

    /// Limit
    private int limit;
    public int Limit
    {
        get => limit;
        set => SetProperty(ref limit, value);
    }

    /// SelectedValue
    private int selectedValue = 0;
    public int SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }

    /// Type
    private StswInfoType type;
    public StswInfoType Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }
}
