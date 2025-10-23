using System.ComponentModel;

namespace StswExpress.Commons;

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
/// Enumeration for <see cref="StswLogItem.Type"/>.
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
/// Enumeration for <see cref="IStswTrackableItem.ItemState"/>.
/// </summary>
public enum StswItemState
{
    Unchanged,
    Added,
    Deleted,
    Modified
}

/// <summary>
/// Enumeration for <see cref="StswFn.MergeObjects(StswMergePriority, object?[])"/>.
/// </summary>
public enum StswMergePriority
{
    First,
    FirstExceptNull,
    Last,
    LastExceptNull,
}

/// <summary>
/// Flags representing planned changes to a feature or element.
/// </summary>
/// <remarks>
/// This enum is copied from the StswExpress.Analyzers project and needs to be kept in sync.
/// </remarks>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[Flags]
public enum StswPlannedChanges
{
    /// <summary>
    /// No planned changes for the feature.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that the feature is under review or reevaluation, and changes are likely but not yet defined.
    /// </summary>
    Revision = 1,

    /// <summary>
    /// Indicates that the feature is incomplete or experimental and planned to be finalized or stabilized in the future.
    /// </summary>
    Finish = 2,

    /// <summary>
    /// Indicates that the feature has known bugs or issues that will be fixed in the future.
    /// </summary>
    Fix = 4,

    /// <summary>
    /// Indicates that the feature is being refactored, meaning it is undergoing internal changes to improve code quality without altering its external behavior.
    /// </summary>
    Refactor = 8,

    /// <summary>
    /// Indicates that the feature is being reworked, which may involve significant changes to its implementation or design.
    /// </summary>
    Rework = 16,

    /// <summary>
    /// Indicates that new features are planned to be added, which may enhance or extend the functionality of the existing feature.
    /// </summary>
    NewFeatures = 32,

    /// <summary>
    /// Indicates that the feature will undergo changes to its logic or behavior, which may alter how it operates or interacts with other components.
    /// </summary>
    LogicChanges = 64,

    /// <summary>
    /// Indicates that there are planned changes to the user interface or visual representation of the feature, which may affect how users interact with it.
    /// </summary>
    VisualChanges = 128,

    /// <summary>
    /// Indicates that the accessibility level of the feature will change, such as changing from public to internal or vice versa.
    /// </summary>
    ChangeAccessibility = 256,

    /// <summary>
    /// Indicates that the name of the feature will change, which may affect how it is referenced or used in code.
    /// </summary>
    ChangeName = 512,

    /// <summary>
    /// Indicates that the feature will be relocated to a different namespace, class, or module, which may affect how it is accessed or organized.
    /// </summary>
    Move = 1024,

    /// <summary>
    /// Indicates that the feature is planned to be removed entirely in a future version. It is recommended to avoid using this feature in new code.
    /// </summary>
    Remove = 2048,
}

/// <summary>
/// Enumeration for <see cref="StswTask.Status"/>.
/// </summary>
public enum StswTaskStatus
{
    Pending,
    Running,
    Completed,
    Faulted,
    Cancelled
}
