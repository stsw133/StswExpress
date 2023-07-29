namespace StswExpress;

/// <summary>
/// Enumeration for <see cref="StswCalendar"/>'s mode.
/// </summary>
public enum StswCalendarMode
{
    ByYear,
    ByMonth
}

/// <summary>
/// Enumeration for <see cref="StswDatabase"/>'s type.
/// </summary>
public enum StswDatabaseType
{
    MSSQL,
    MySQL,
    PostgreSQL
}

/// <summary>
/// Enumeration for <see cref="StswDatePicker"/>'s increment type.
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
/// Enumeration for <see cref="StswContentDialog"/>'s buttons.
/// </summary>
public enum StswDialogButtons
{
    OK,
    OKCancel,
    YesNoCancel,
    YesNo
}

/// <summary>
/// Enumeration for <see cref="StswContentDialog"/>'s image.
/// </summary>
public enum StswDialogImage
{
    None,
    Error,
    Information,
    Question,
    Success,
    Warning
}

/// <summary>
/// Enumeration for <see cref="StswFilter"/>'s mode.
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
/// Enumeration for <see cref="StswFilter"/>'s type.
/// </summary>
public enum StswFilterType
{
    Check,
    Date,
    List,
    Number,
    Text
}

/// <summary>
/// Enumeration for <see cref="StswLogItem"/>'s type.
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
/// Enumeration for <see cref="StswImage"/>'s menu mode.
/// </summary>
public enum StswMenuMode
{
    Disabled,
    ReadOnly,
    Full
}

/// <summary>
/// Enumeration for <see cref="StswProgressBar"/>'s states.
/// </summary>
public enum StswProgressState
{
    Ready,
    Running,
    Paused,
    Error
}

/// <summary>
/// Enumeration for <see cref="StswProgressBar"/>'s text mode.
/// </summary>
public enum StswProgressTextMode
{
    None,
    Value,
    Percentage
}

/// <summary>
/// Enumerator for <see cref="StswDataGrid"/>'s special column visibility.
/// </summary>
public enum StswSpecialColumnVisibility
{
    Collapsed,
    All,
    OnlyRows
}
