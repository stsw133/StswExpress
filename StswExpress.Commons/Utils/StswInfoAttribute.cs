using System.ComponentModel;

namespace StswExpress.Commons;

/// <summary>
/// Flags to indicate planned changes for a feature.
/// </summary>
/// <param name="sinceVersion">The version since this feature is available. Can be <see langword="null"/> if not applicable.</param>
/// <param name="lastUpdateVersion">The version when the feature was last updated or changed. Can be <see langword="null"/> if not applicable.</param>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(
      AttributeTargets.Class
    | AttributeTargets.Constructor
    | AttributeTargets.Enum
    | AttributeTargets.Field
    | AttributeTargets.Interface
    | AttributeTargets.Method
    | AttributeTargets.Property
    | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
[StswInfo("0.19.0", "0.20.0")]
public sealed class StswInfoAttribute(string? sinceVersion, string? lastUpdateVersion = null) : Attribute
{
    /// <summary>
    /// Indicates the version since this feature is available.
    /// </summary>
    public string? SinceVersion { get; init; } = sinceVersion;

    /// <summary>
    /// Indicates the version when the feature was last updated or changed.
    /// </summary>
    public string? LastUpdateVersion { get; init; } = lastUpdateVersion;

    /// <summary>
    /// Indicates if the feature has been tested, meaning it was used and verified for most common scenarios.
    /// </summary>
    public bool IsTested { get; init; } = true;

    /// <summary>
    /// Indicates if there are any planned changes for this feature in the future.
    /// </summary>
    public StswPlannedChanges Changes { get; init; } = StswPlannedChanges.None;
}
