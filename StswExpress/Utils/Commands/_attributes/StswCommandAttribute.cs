using System;

namespace StswExpress;

/// <summary>
/// Attribute to mark methods as synchronous commands.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class StswCommandAttribute : Attribute
{
}
