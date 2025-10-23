namespace StswExpress.Commons;

/// <summary>
/// Attribute to mark a field as an observable property.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[StswPlannedChanges(StswPlannedChanges.LogicChanges, "This attribute may be extended in the future to include more options like boolean flag to copy attributes.")]
public class StswObservablePropertyAttribute : Attribute
{
    public string[]? NotifyProperties { get; set; }
    public string? ConditionMethod { get; set; }
}
