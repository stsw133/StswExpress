using System.Linq;

namespace TestApp;

public class StswFilterBoxContext : ControlsContext
{
    public StswCommand<ControlsBase?> RefreshCommand => new(Refresh);

    public override void SetDefaults()
    {
        base.SetDefaults();

        FilterMenuMode = (StswMenuMode?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(FilterMenuMode)))?.Value ?? default;
        FilterType = (StswAdaptiveType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(FilterType)))?.Value ?? default;
        IsFilterCaseSensitive = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsFilterCaseSensitive)))?.Value ?? default;
        IsFilterNullSensitive = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsFilterNullSensitive)))?.Value ?? default;
    }

    #region Events & methods
    /// Command: refresh
    private void Refresh(ControlsBase? controlsBase)
    {
        if (controlsBase?.Content is StswFilterBox filter)
        {
            SqlParam1 = filter.SqlParam + '1';
            SqlParam2 = filter.SqlParam + '2';
            SqlString = filter.SqlString;
            Value1 = filter.Value1;
            Value2 = filter.Value2;
        }
    }
    #endregion

    /// FilterMenuMode
    public StswMenuMode FilterMenuMode
    {
        get => _filterMenuMode;
        set => SetProperty(ref _filterMenuMode, value);
    }
    private StswMenuMode _filterMenuMode;

    /// FilterType
    public StswAdaptiveType FilterType
    {
        get => _filterType;
        set => SetProperty(ref _filterType, value);
    }
    private StswAdaptiveType _filterType;

    /// IsFilterCaseSensitive
    public bool IsFilterCaseSensitive
    {
        get => _isFilterCaseSensitive;
        set => SetProperty(ref _isFilterCaseSensitive, value);
    }
    private bool _isFilterCaseSensitive;

    /// IsFilterNullSensitive
    public bool IsFilterNullSensitive
    {
        get => _isFilterNullSensitive;
        set => SetProperty(ref _isFilterNullSensitive, value);
    }
    private bool _isFilterNullSensitive;

    /// SqlParam
    public string SqlParam1
    {
        get => _sqlParam1;
        set => SetProperty(ref _sqlParam1, value);
    }
    private string _sqlParam1 = string.Empty;

    public string SqlParam2
    {
        get => _sqlParam2;
        set => SetProperty(ref _sqlParam2, value);
    }
    private string _sqlParam2 = string.Empty;

    /// SqlString
    public string? SqlString
    {
        get => _sqlString;
        set => SetProperty(ref _sqlString, value);
    }
    private string? _sqlString = string.Empty;

    /// Value
    public object? Value1
    {
        get => _value1;
        set => SetProperty(ref _value1, value);
    }
    private object? _value1;

    public object? Value2
    {
        get => _value2;
        set => SetProperty(ref _value2, value);
    }
    private object? _value2;
}
