namespace StswExpress.Commons;

/// <summary>
/// Represents an error returned by an SMTP server or raised while communicating with it.
/// </summary>
public class StswSmtpException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="StswSmtpException"/> class.
	/// </summary>
	public StswSmtpException() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="StswSmtpException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	public StswSmtpException(string message) : base(message) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="StswSmtpException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The inner exception.</param>
	public StswSmtpException(string message, Exception innerException) : base(message, innerException) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="StswSmtpException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="response">The SMTP response associated with the error.</param>
	/// <param name="command">The SMTP command associated with the error.</param>
	public StswSmtpException(string message, StswSmtpResponse? response, string? command = null) : base(BuildMessage(message, response, command))
	{
		Response = response;
		Command = command;
	}

	/// <summary>
	/// Gets the SMTP response associated with the error.
	/// </summary>
	public StswSmtpResponse? Response { get; }

	/// <summary>
	/// Gets the SMTP command associated with the error.
	/// </summary>
	public string? Command { get; }

	private static string BuildMessage(string message, StswSmtpResponse? response, string? command)
	{
		if (response is null)
			return message;

		var commandPart = string.IsNullOrWhiteSpace(command) ? null : $" Command: {command}.";
		return $"{message}{commandPart} Response: {response}.";
	}
}
