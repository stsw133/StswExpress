namespace StswExpress.Commons;

[AttributeUsage(AttributeTargets.Field)]
public class StswObservablePropertyAttribute : Attribute
{
    public string[]? NotifyProperties { get; set; }
    public string? ConditionMethod { get; set; }
}
