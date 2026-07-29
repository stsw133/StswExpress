using System;

namespace StswExpress.Wpf;

/// <summary>
/// Attribute to mark methods as either sync or async commands.
/// </summary>
/// <param name="conditionMethodName">Optional name of the method that determines whether the command can execute.</param>
/// <param name="isReusable">Determines whether an asynchronous command can be executed again while it is still running.</param>
[AttributeUsage(AttributeTargets.Method)]
public class StswCommandAttribute(string? conditionMethodName = null, bool isReusable = false) : Attribute
{
	/// <summary>
	/// Gets or sets the name of the method that determines whether the command can execute.
	/// </summary>
	public string? ConditionMethodName { get; set; } = conditionMethodName;

	/// <summary>
	/// Gets or sets a value indicating whether an asynchronous command can be executed again while it is still running.
	/// </summary>
	public bool IsReusable { get; set; } = isReusable;

	/// <summary>
	/// Gets or sets the log targets used when an exception is thrown while executing the generated command.
	/// The default value, <see cref="StswLogTarget.None"/>, disables generated try/catch handling.
	/// </summary>
	/// <example>
	/// <code>
	/// [StswCommand(TryCatch = StswLogTarget.MessageDialog)]
	/// private async Task SaveAsync()
	/// {
	///     // Command body.
	/// }
	/// </code>
	/// </example>
	public StswLogTarget TryCatch { get; set; } = StswLogTarget.None;
}
