namespace StswExpress;

/// <summary>
/// Enumeration for <see cref="StswAdaptiveBox.Type"/> and <see cref="StswFilterBox.FilterType"/>.
/// </summary>
public enum StswAdaptiveType
{
    Auto = -1,
    Check,
    Date,
    List,
    Number,
    Text
}

/// <summary>
/// Enumeration for <see cref="StswGrid.AutoLayoutMode"/>.
/// </summary>
public enum StswAutoLayoutMode
{
    None,
    AutoDefinitions,
    IncrementColumns,
    IncrementRows
}

/// <summary>
/// Enumeration for <see cref="StswCalendar.SelectionMode"/>.
/// </summary>
public enum StswCalendarMode
{
    Days,
    Months
}

/// <summary>
/// Enumeration for <see cref="StswNavigation.TabStripMode"/> and <see cref="StswTextEditor.ToolbarMode"/>.
/// </summary>
public enum StswCompactibility
{
    Collapsed,
    Compact,
    Full
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
/// Enumeration for <see cref="StswDataGrid.FiltersType"/>.
/// </summary>
public enum StswDataGridFiltersType
{
    CollectionView,
    SQL
}

/// <summary>
/// Enumeration for <see cref="StswDatePicker.IncrementType"/>.
/// </summary>
public enum StswDateTimeIncrementType
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
    Blockade,
    Debug,
    Error,
    Fatal,
    Information,
    Question,
    Success,
    Warning
}

/// <summary>
/// Enumeration for <see cref="StswFilterBox.FilterMode"/>.
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
/// Enumeration for <see cref="StswDatabaseModel.Set"/>.
/// </summary>
public enum StswInclusionMode
{
    Exclude,
    Include
}

/// <summary>
/// Enumeration for <see cref="StswInfoBadge.Format"/>.
/// </summary>
public enum StswInfoFormat
{
    Dot,
    Icon,
    Number
}

/// <summary>
/// Enumeration for <see cref="StswInfoBadge.Type"/>,
/// <see cref="StswInfoBar.Type"/> and <see cref="StswLogItem.Type"/>.
/// </summary>
public enum StswInfoType
{
    None,
    Debug,
    Error,
    Fatal,
    Information,
    Success,
    Warning
}

/// <summary>
/// Enumeration for <see cref="IStswCollectionItem.ItemState"/>.
/// </summary>
public enum StswItemState
{
    Unchanged,
    Added,
    Deleted,
    Modified
}

/// <summary>
/// Enumeration for <see cref="StswFilterBox.FilterMenuMode"/> and <see cref="StswImage.MenuMode"/>.
/// </summary>
public enum StswMenuMode
{
    Disabled,
    Full,
    ReadOnly
}

/// <summary>
/// Enumeration for <see cref="StswNotifyIcon.Notify"/>.
/// </summary>
public enum StswNotificationType
{
    None,
    Info,
    Warning,
    Error
}

/// <summary>
/// Enumeration for <see cref="StswPathPicker.SelectionMode"/>.
/// </summary>
public enum StswPathType
{
    Directory,
    File
}

/// <summary>
/// Enumeration for <see cref="StswWindow.ConfigPresentationMode"/>.
/// </summary>
public enum StswPresentationMode
{
    ContentDialog,
    Window
}

/// <summary>
/// Enumeration for <see cref="StswProgressBar.State"/> and <see cref="StswProgressRing.State"/>.
/// </summary>
public enum StswProgressState
{
    Custom = -1,
    Ready,
    Running,
    Paused,
    Error,
    Finished
}

/// <summary>
/// Enumeration for <see cref="StswProgressBar.TextMode"/> and <see cref="StswProgressRing.TextMode"/>.
/// </summary>
public enum StswProgressTextMode
{
    Custom = -1,
    None,
    Percentage,
    Progress,
    Value
}

/// <summary>
/// Enumerator for <see cref="StswPopup.ScrollType"/>.
/// </summary>
public enum StswScrollType
{
    DirectionView,
    ScrollView
}

/// <summary>
/// Enumerator for <see cref="StswSpinner.Type"/>.
/// </summary>
public enum StswSpinnerType
{
    Circles,
    Crescent,
    Dots,
    Lines
}

/// <summary>
/// Enumeration for <see cref="StswResources.Theme"/>.
/// </summary>
public enum StswTheme
{
    Auto = -1,
    Light,
    Dark,
    Pink,
    Spring
}

/// <summary>
/// Enumeration for <see cref="StswTimePicker.IncrementType"/>.
/// </summary>
public enum StswTimeSpanIncrementType
{
    None,
    Day,
    Hour,
    Minute,
    Second
}
