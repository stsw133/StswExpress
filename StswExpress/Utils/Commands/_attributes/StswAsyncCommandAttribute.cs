using System;

namespace StswExpress;

/// <summary>
/// Attribute to mark methods as asynchronous commands.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Stsw("0.19.0", Changes = StswPlannedChanges.None)]
public class StswAsyncCommandAttribute : Attribute
{
    public bool IsReusable { get; set; } = false;
}
