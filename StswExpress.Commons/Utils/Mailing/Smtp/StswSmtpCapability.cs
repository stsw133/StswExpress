namespace StswExpress.Commons;

/// <summary>
/// Defines SMTP capabilities supported by the connected server.
/// </summary>
[Flags]
public enum StswSmtpCapability
{
	/// <summary>
	/// No known capabilities.
	/// </summary>
	None = 0,

	/// <summary>
	/// The server supports STARTTLS.
	/// </summary>
	StartTls = 1 << 0,

	/// <summary>
	/// The server supports authentication.
	/// </summary>
	Authentication = 1 << 1,

	/// <summary>
	/// The server supports AUTH PLAIN.
	/// </summary>
	AuthPlain = 1 << 2,

	/// <summary>
	/// The server supports AUTH LOGIN.
	/// </summary>
	AuthLogin = 1 << 3,

	/// <summary>
	/// The server supports 8BITMIME.
	/// </summary>
	EightBitMime = 1 << 4,

	/// <summary>
	/// The server announces a maximum message size.
	/// </summary>
	Size = 1 << 5,

	/// <summary>
	/// The server supports SMTPUTF8.
	/// </summary>
	SmtpUtf8 = 1 << 6,

	/// <summary>
	/// The server supports command pipelining.
	/// </summary>
	Pipelining = 1 << 7,
}
