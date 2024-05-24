using System.Linq;

namespace TestApp;

public class StswPopupContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        ScrollType = (StswScrollType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(ScrollType)))?.Value ?? default;
    }

    /// IsOpen
    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }
    private bool _isOpen = false;

    /// ScrollType
    public StswScrollType ScrollType
    {
        get => _scrollType;
        set => SetProperty(ref _scrollType, value);
    }
    private StswScrollType _scrollType;
}
