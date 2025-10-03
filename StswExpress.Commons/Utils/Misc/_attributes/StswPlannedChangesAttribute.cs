namespace StswExpress.Commons;

/// <summary>
/// Indicates planned changes to a feature or element.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class StswPlannedChangesAttribute(StswPlannedChanges plannedChanges) : Attribute
{
    /// <summary>
    /// Gets the planned changes.
    /// </summary>
    public StswPlannedChanges PlannedChanges { get; } = plannedChanges;
}
