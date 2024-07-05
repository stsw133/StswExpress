using System;
using System.Linq;

namespace TestApp;

public class StswDecimalBoxContext : ControlsContext
{
    public StswCommand ClearCommand => new(() => SelectedValue = default);
    public StswCommand RandomizeCommand => new(() => SelectedValue = new Random().Next(int.MinValue, int.MaxValue));

    public override void SetDefaults()
    {
        base.SetDefaults();

        Increment = (decimal?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Increment)))?.Value ?? default;
        IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? default;
    }

    /// Format
    public string? Format
    {
        get => _format;
        set => SetProperty(ref _format, value);
    }
    private string? _format = "N2";

    /// Increment
    public decimal Increment
    {
        get => _increment;
        set => SetProperty(ref _increment, value);
    }
    private decimal _increment;

    /// IsReadOnly
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }
    private bool _isReadOnly;

    /// Maximum
    public decimal? Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }
    private decimal? _maximum;

    /// Minimum
    public decimal? Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }
    private decimal? _minimum;

    /// SelectedValue
    public decimal? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
    private decimal? _selectedValue = 0;
    
    /// SubControls
    public bool SubControls
    {
        get => _subControls;
        set => SetProperty(ref _subControls, value);
    }
    private bool _subControls = false;
}
