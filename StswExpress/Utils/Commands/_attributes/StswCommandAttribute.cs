using System;

namespace StswExpress;

/// <summary>
/// Attribute to mark methods as either sync or async commands.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class StswCommandAttribute(string? conditionMethodName = null, bool isReusable = false) : Attribute
{
    public string? ConditionMethodName { get; set; } = conditionMethodName;
    public bool IsReusable { get; set; } = isReusable;
}
