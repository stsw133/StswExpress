namespace StswExpress.Commons;

/// <summary>
/// Defines how the SMTP connection should be secured.
/// </summary>
public enum StswMailSecurityOption
{
	/// <summary>
	/// Does not use TLS or SSL.
	/// </summary>
	None = 0,

	/// <summary>
	/// Automatically selects the most suitable security mode based on the SMTP port and server capabilities.
	/// </summary>
	Auto = 1,

	/// <summary>
	/// Uses TLS immediately after connecting to the server. Commonly used with port 465.
	/// </summary>
	SslOnConnect = 2,

	/// <summary>
	/// Requires STARTTLS after connecting to the server. Commonly used with port 587.
	/// </summary>
	StartTls = 3,

	/// <summary>
	/// Uses STARTTLS if the server supports it, otherwise continues without TLS.
	/// </summary>
	StartTlsWhenAvailable = 4,
}