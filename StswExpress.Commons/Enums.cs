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
/// Enumeration for <see cref="StswFn.MergeObjects(StswMergePriority, object?[])"/>.
/// </summary>
public enum StswMergePriority
{
    First,
    Last,
    LastExceptNull,
}
