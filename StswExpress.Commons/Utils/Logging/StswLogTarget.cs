namespace StswExpress.Commons;

/// <summary>
/// Specifies the enabled log output targets.
/// </summary>
[Flags]
public enum StswLogTarget
{
	/// <summary>
	/// Logging is disabled for the selected call or configuration.
	/// </summary>
	None = 0,

	/// <summary>
	/// Writes logs to the daily log file.
	/// </summary>
	File = 1,

	/// <summary>
	/// Writes logs to Windows Event Viewer.
	/// </summary>
	EventViewer = 2,

	/// <summary>
	/// Shows logs through StswExpress.Wpf message dialog/message box when available at runtime.
	/// </summary>
	MessageBox = 4,

	/// <summary>
	/// Runs custom logging delegates/events configured on <see cref="StswLog"/> or <see cref="StswLogConfig"/>.
	/// </summary>
	Custom = 8,
}
