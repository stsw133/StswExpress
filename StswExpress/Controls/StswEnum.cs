namespace StswExpress;

/// <summary>
/// Enumeration for <see cref="StswAdaptiveBox.Type"/> and for <see cref="StswFilter.FilterType"/>.
/// </summary>
public enum StswAdaptiveType
{
    Check,
    Date,
    List,
    Number,
    Text
}

/// <summary>
/// Enumeration for <see cref="StswCalendar.SelectionMode"/>.
/// </summary>
public enum StswCalendarMode
{
    ByMonth,
    ByYear
}

/// <summary>
/// Enumeration for <see cref="StswDatabaseModel.Type"/>.
/// </summary>
public enum StswDatabaseType
{
    MSSQL,
    MySQL,
    PostgreSQL
}

/// <summary>
/// Enumeration for <see cref="StswDatePicker.IncrementType"/>.
/// </summary>
public enum StswDateIncrementType
{
    None,
    Year,
    Month,
    Day,
    Hour,
    Minute,
    Second
}

/// <summary>
/// Enumeration for <see cref="StswMessageDialog.Buttons"/>.
/// </summary>
public enum StswDialogButtons
{
    OK,
    OKCancel,
    YesNoCancel,
    YesNo
}

/// <summary>
/// Enumeration for <see cref="StswMessageDialog.Image"/>.
/// </summary>
public enum StswDialogImage
{
    None,
    Debug,
    Error,
    Information,
    Question,
    Success,
    Warning
}

/// <summary>
/// Enumeration for <see cref="StswFilter.FilterMode"/>.
/// </summary>
public enum StswFilterMode
{
    Equal,
    NotEqual,
    Greater,
    GreaterEqual,
    Less,
    LessEqual,
    Between,
    Contains,
    NotContains,
    Like,
    NotLike,
    StartsWith,
    EndsWith,
    In,
    NotIn,
    Null,
    NotNull
}

/// <summary>
/// Enumeration for <see cref="StswLogItem.Type"/>.
/// </summary>
public enum StswLogType
{
    None,
    Debug,
    Error,
    Information,
    Success,
    Warning
}

/// <summary>
/// Enumeration for <see cref="StswImage.MenuMode"/>.
/// </summary>
public enum StswMenuMode
{
    Disabled,
    Full,
    ReadOnly
}

/// <summary>
/// Enumeration for <see cref="StswProgressBar.State"/>.
/// </summary>
public enum StswProgressState
{
    Ready,
    Running,
    Paused,
    Error,
    Finished
}

/// <summary>
/// Enumeration for <see cref="StswProgressBar.TextMode"/>.
/// </summary>
public enum StswProgressTextMode
{
    Custom = -1,
    None,
    Percentage,
    Value
}

/// <summary>
/// Enumerator for <see cref="StswDataGrid.SpecialColumnVisibility"/>.
/// </summary>
public enum StswSpecialColumnVisibility
{
    Collapsed,
    All,
    OnlyRows
}

/// <summary>
/// Enumeration for <see cref="StswResources.Theme"/>.
/// </summary>
public enum StswTheme
{
    Auto = -1,
    Light,
    Dark,
    Pink
}

/// <summary>
/// Enumeration for <see cref="StswTextEditor.ToolbarMode"/>.
/// </summary>
public enum StswToolbarMode
{
    Collapsed,
    Compact,
    Full
}
