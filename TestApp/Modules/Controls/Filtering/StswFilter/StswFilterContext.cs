﻿namespace TestApp;

public class StswFilterContext : ControlsContext
{
    #region Properties
    /// FilterType
    private StswFilterType filterType = StswFilterType.Text;
    public StswFilterType FilterType
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
    #endregion
}
