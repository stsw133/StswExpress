using System.Linq;

namespace TestApp;
public partial class StswPopupContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ScrollType = (StswScrollType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ScrollType)))?.Value ?? default;
    }

    [StswObservableProperty] bool _isOpen = false;
    [StswObservableProperty] StswScrollType _scrollType;
}
