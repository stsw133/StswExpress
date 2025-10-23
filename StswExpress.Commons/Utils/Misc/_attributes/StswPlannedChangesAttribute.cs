namespace StswExpress.Commons;

/// <summary>
/// Indicates planned changes to a feature or element.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class StswPlannedChangesAttribute(StswPlannedChanges plannedChanges, string? reason = null) : Attribute
{
    /// <summary>
    /// Gets the planned changes.
    /// </summary>
    public StswPlannedChanges PlannedChanges { get; } = plannedChanges;

    /// <summary>
    /// Gets the reason for the planned changes, if provided.
    /// </summary>
    public string? Reason { get; } = reason;
}
