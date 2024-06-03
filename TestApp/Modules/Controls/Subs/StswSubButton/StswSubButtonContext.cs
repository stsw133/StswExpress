using System.Linq;
using System.Windows;

namespace TestApp;

public class StswSubButtonContext : ControlsContext
{
    public StswCommand OnClickCommand => new(() => ClickCounter++);
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
    }

    /// ClickCounter
    public int ClickCounter
    {
        get => _clickCounter;
        set => SetProperty(ref _clickCounter, value);
    }
    private int _clickCounter;

    /// IconScale
    public GridLength IconScale
    {
        get => _iconScale;
        set => SetProperty(ref _iconScale, value);
    }
    private GridLength _iconScale;

    /// IsBusy
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// IsContentVisible
    public bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetProperty(ref _isContentVisible, value);
    }
    private bool _isContentVisible;
}
