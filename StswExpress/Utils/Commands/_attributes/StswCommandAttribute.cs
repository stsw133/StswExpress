﻿using System;

namespace StswExpress;

/// <summary>
/// Attribute to mark methods as either sync or async commands.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[StswInfo("0.19.0")]
public class StswCommandAttribute : Attribute
{
    public string? ConditionMethodName { get; set; }
    public bool IsReusable { get; set; }
}
