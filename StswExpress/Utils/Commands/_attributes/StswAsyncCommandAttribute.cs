using System;

namespace StswExpress;

/// <summary>
/// Attribute to mark methods as asynchronous commands.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class StswAsyncCommandAttribute : Attribute
{
    public bool IsReusable { get; set; } = false;
}
