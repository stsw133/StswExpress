using System.Linq;

namespace TestApp;
public partial class StswFilterBoxContext : ControlsContext
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

    [StswObservableProperty] StswMenuMode _filterMenuMode;
    [StswObservableProperty] StswAdaptiveType _filterType;
    [StswObservableProperty] bool _isFilterCaseSensitive;
    [StswObservableProperty] bool _isFilterNullSensitive;
    [StswObservableProperty] string _sqlParam1 = string.Empty;
    [StswObservableProperty] string _sqlParam2 = string.Empty;
    [StswObservableProperty] string? _sqlString = string.Empty;
    [StswObservableProperty] object? _value1;
    [StswObservableProperty] object? _value2;
}
