namespace StswExpress.Commons;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class StswObservablePropertyAttribute : Attribute
{
    public string[]? NotifyProperties { get; set; }
    public string? CallbackMethod { get; set; }
    public string? ConditionMethod { get; set; }
}
