using System.Linq;

namespace TestApp;

public class StswProgressRingContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsIndeterminate = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsIndeterminate)))?.Value ?? default;
        State = (StswProgressState?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(State)))?.Value ?? default;
        TextMode = (StswProgressTextMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TextMode)))?.Value ?? default;
    }

    /// IsIndeterminate
    private bool isIndeterminate;
    public bool IsIndeterminate
    {
        get => isIndeterminate;
        set => SetProperty(ref isIndeterminate, value);
    }

    /// Maximum
    private double? maximum = 100;
    public double? Maximum
    {
        get => maximum;
        set => SetProperty(ref maximum, value);
    }
    /// Minimum
    private double? minimum = 0;
    public double? Minimum
    {
        get => minimum;
        set => SetProperty(ref minimum, value);
    }

    /// SelectedValue
    private double? selectedValue = 0;
    public double? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }

    /// State
    private StswProgressState state;
    public StswProgressState State
    {
        get => state;
        set => SetProperty(ref state, value);
    }

    /// TextMode
    private StswProgressTextMode textMode;
    public StswProgressTextMode TextMode
    {
        get => textMode;
        set => SetProperty(ref textMode, value);
    }
}
