using System;
using System.Linq;

namespace TestApp;

public class StswNumericBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedValue = default);
    public StswCommand RandomizeCommand => new(() => SelectedValue = new Random().Next(int.MinValue, int.MaxValue));

    public override void SetDefaults()
    {
        base.SetDefaults();

        Increment = (decimal?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Increment)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    #region Properties
    /// Components
    private bool components = false;
    public bool Components
    {
        get => components;
        set => SetProperty(ref components, value);
    }

    /// Format
    private string? format = "N2";
    public string? Format
    {
        get => format;
        set => SetProperty(ref format, value);
    }

    /// Increment
    private decimal increment;
    public decimal Increment
    {
        get => increment;
        set => SetProperty(ref increment, value);
    }

    /// IsReadOnly
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => SetProperty(ref isReadOnly, value);
    }

    /// Maximum
    private decimal? maximum;
    public decimal? Maximum
    {
        get => maximum;
        set => SetProperty(ref maximum, value);
    }
    /// Minimum
    private decimal? minimum;
    public decimal? Minimum
    {
        get => minimum;
        set => SetProperty(ref minimum, value);
    }

    /// SelectedValue
    private decimal? selectedValue = 0;
    public decimal? SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }
    #endregion
}
