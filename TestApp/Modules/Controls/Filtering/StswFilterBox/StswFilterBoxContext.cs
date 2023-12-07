using System.Windows.Input;

namespace TestApp;

public class StswFilterBoxContext : ControlsContext
{
    public ICommand RefreshCommand { get; set; }

    public StswFilterBoxContext()
    {
        RefreshCommand = new StswCommand<ControlsBase?>(Refresh);
    }

    #region Events and methods
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

    #region Properties
    /// FilterType
    private StswAdaptiveType filterType = StswAdaptiveType.Text;
    public StswAdaptiveType FilterType
    {
        get => filterType;
        set => SetProperty(ref filterType, value);
    }

    /// IsFilterNullSensitive
    private bool isFilterNullSensitive = false;
    public bool IsFilterNullSensitive
    {
        get => isFilterNullSensitive;
        set => SetProperty(ref isFilterNullSensitive, value);
    }

    /// SqlParam
    private string sqlParam1 = string.Empty;
    public string SqlParam1
    {
        get => sqlParam1;
        set => SetProperty(ref sqlParam1, value);
    }
    private string sqlParam2 = string.Empty;
    public string SqlParam2
    {
        get => sqlParam2;
        set => SetProperty(ref sqlParam2, value);
    }

    /// SqlString
    private string? sqlString = string.Empty;
    public string? SqlString
    {
        get => sqlString;
        set => SetProperty(ref sqlString, value);
    }

    /// Value
    private object? value1;
    public object? Value1
    {
        get => value1;
        set => SetProperty(ref value1, value);
    }
    private object? value2;
    public object? Value2
    {
        get => value2;
        set => SetProperty(ref value2, value);
    }
    #endregion
}
