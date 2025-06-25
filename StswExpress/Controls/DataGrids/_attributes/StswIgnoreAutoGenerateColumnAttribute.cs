using System;

namespace StswExpress;

/// <summary>
/// Specifies that the marked property should be ignored when automatically generating columns 
/// in a <see cref="StswDataGrid"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StswIgnoreAutoGenerateColumnAttribute : Attribute
{
}
