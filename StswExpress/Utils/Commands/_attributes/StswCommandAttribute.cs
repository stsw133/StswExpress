using System;

namespace StswExpress;

/// <summary>
/// Attribute to mark methods as synchronous commands.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Stsw("0.19.0", Changes = StswPlannedChanges.None)]
public class StswCommandAttribute : Attribute
{
}
