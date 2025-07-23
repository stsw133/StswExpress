namespace StswExpress.Commons;

/// <summary>
/// Attribute to mark a field as an observable property.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[StswInfo("0.19.0", Changes = StswPlannedChanges.LogicChanges)]
public class StswObservablePropertyAttribute : Attribute
{
    public string[]? NotifyProperties { get; set; }
    public string? ConditionMethod { get; set; }
}
