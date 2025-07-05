using System.ComponentModel;

namespace StswExpress.Commons;

/// <summary>
/// Attribute to mark features in the StswExpress library.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[Stsw("0.19.0", Changes = StswPlannedChanges.None)]
public sealed class StswAttribute(string? sinceVersion) : Attribute
{
    /// <summary>
    /// The version of the library when this feature was added.
    /// </summary>
    public string? SinceVersion { get; } = sinceVersion;

    /// <summary>
    /// Indicates if the feature has been tested, meaning it was used and verified for most common scenarios.
    /// </summary>
    public bool IsTested { get; init; } = true;

    /// <summary>
    /// Indicates if there are any planned changes for this feature in the future.
    /// </summary>
    public StswPlannedChanges Changes { get; init; } = StswPlannedChanges.None;
}
