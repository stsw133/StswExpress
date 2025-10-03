namespace StswExpress.Commons;

/// <summary>
/// Attribute to mark a field as an observable property.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[StswPlannedChanges(StswPlannedChanges.LogicChanges)]
public class StswObservablePropertyAttribute : Attribute
{
    public string[]? NotifyProperties { get; set; }
    public string? ConditionMethod { get; set; }
}
