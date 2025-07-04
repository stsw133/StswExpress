using System.Linq;

namespace TestApp;
public partial class StswInfoBadgeContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Format = (StswInfoFormat?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Format)))?.Value ?? default;
        Limit = (int?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Limit)))?.Value ?? default(int?);
        Type = (StswInfoType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    [StswObservableProperty] StswInfoFormat _format;
    [StswObservableProperty] int? _limit;
    [StswObservableProperty] int _selectedValue = 0;
    [StswObservableProperty] StswInfoType _type;
}
