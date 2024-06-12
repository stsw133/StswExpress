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
    public StswInfoFormat Format
    {
        get => _format;
        set => SetProperty(ref _format, value);
    }
    private StswInfoFormat _format;

    /// Limit
    public int? Limit
    {
        get => _limit;
        set => SetProperty(ref _limit, value);
    }
    private int? _limit;

    /// SelectedValue
    public int SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
    private int _selectedValue = 0;

    /// Type
    public StswInfoType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private StswInfoType _type;
}
