using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Provides a lightweight SMTP client with support for plain SMTP, STARTTLS, implicit TLS, and basic authentication.
/// </summary>
public sealed class StswSmtpClient : IDisposable
{
	private TcpClient? _tcpClient;
	private Stream? _stream;
	private StreamReader? _reader;
	private bool _disposed;

	/// <summary>
	/// Gets or sets the local domain used in EHLO/HELO.
	/// </summary>
	public string? LocalDomain { get; set; }

	/// <summary>
	/// Gets or sets the connection timeout in milliseconds.
	/// </summary>
	public int Timeout { get; set; } = 100_000;

	/// <summary>
	/// Gets or sets the server certificate validation callback.
	/// </summary>
	public RemoteCertificateValidationCallback? ServerCertificateValidationCallback { get; set; }

	/// <summary>
	/// Gets the SMTP capabilities advertised by the server.
	/// </summary>
	public StswSmtpCapability Capabilities { get; private set; }

	/// <summary>
	/// Gets the maximum message size advertised by the server, if available.
	/// </summary>
	public long? MaxMessageSize { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the connection is secured with TLS.
	/// </summary>
	public bool IsSecure { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the client is connected.
	/// </summary>
	public bool IsConnected => _tcpClient?.Connected == true && _stream is not null;

	/// <summary>
	/// Connects to an SMTP server.
	/// </summary>
	/// <param name="host">The SMTP host.</param>
	/// <param name="port">The SMTP port.</param>
	/// <param name="securityOption">The security option.</param>
	public void Connect(string host, int port, StswMailSecurityOption securityOption) => ConnectAsync(host, port, securityOption).GetAwaiter().GetResult();

	/// <summary>
	/// Connects to an SMTP server.
	/// </summary>
	/// <param name="host">The SMTP host.</param>
	/// <param name="port">The SMTP port.</param>
	/// <param name="securityOption">The security option.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	public async Task ConnectAsync(string host, int port, StswMailSecurityOption securityOption, CancellationToken cancellationToken = default)
	{
		ThrowIfDisposed();

		if (string.IsNullOrWhiteSpace(host))
			throw new ArgumentException("SMTP host cannot be empty.", nameof(host));

		if (IsConnected)
			throw new InvalidOperationException("The SMTP client is already connected.");

		var resolvedSecurityOption = ResolveSecurityOption(port, securityOption);

		try
		{
			_tcpClient = new TcpClient
			{
				NoDelay = true,
				SendTimeout = Timeout,
				ReceiveTimeout = Timeout,
			};

			cancellationToken.ThrowIfCancellationRequested();
			await _tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
			_stream = _tcpClient.GetStream();

			if (resolvedSecurityOption == StswMailSecurityOption.SslOnConnect)
				await UpgradeToTlsAsync(host, cancellationToken).ConfigureAwait(false);
			else
				ResetReader();

			var greeting = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			EnsureResponse(greeting, "server greeting", 220);

			await SendEhloOrHeloAsync(cancellationToken).ConfigureAwait(false);

			if (resolvedSecurityOption is StswMailSecurityOption.StartTls or StswMailSecurityOption.StartTlsWhenAvailable)
			{
				var supportsStartTls = Capabilities.HasFlag(StswSmtpCapability.StartTls);

				if (!supportsStartTls && resolvedSecurityOption == StswMailSecurityOption.StartTls)
					throw new StswSmtpException("The SMTP server does not support STARTTLS, but STARTTLS is required.", null, "STARTTLS");

				if (supportsStartTls)
				{
					await SendCommandAsync("STARTTLS", [220], cancellationToken).ConfigureAwait(false);
					await UpgradeToTlsAsync(host, cancellationToken).ConfigureAwait(false);
					await SendEhloOrHeloAsync(cancellationToken).ConfigureAwait(false);
				}
			}
		}
		catch
		{
			Close();
			throw;
		}
	}

	/// <summary>
	/// Authenticates using the best supported basic SMTP authentication mechanism.
	/// </summary>
	/// <param name="username">The username.</param>
	/// <param name="password">The password.</param>
	public void Authenticate(string username, string? password) => AuthenticateAsync(username, password).GetAwaiter().GetResult();

	/// <summary>
	/// Authenticates using the best supported basic SMTP authentication mechanism.
	/// </summary>
	/// <param name="username">The username.</param>
	/// <param name="password">The password.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	public async Task AuthenticateAsync(string username, string? password, CancellationToken cancellationToken = default)
	{
		ThrowIfDisposed();
		EnsureConnected();

		if (string.IsNullOrWhiteSpace(username))
			throw new ArgumentException("SMTP username cannot be empty.", nameof(username));

		password ??= string.Empty;

		if (Capabilities.HasFlag(StswSmtpCapability.AuthPlain))
		{
			await AuthenticatePlainAsync(username, password, cancellationToken).ConfigureAwait(false);
			return;
		}

		if (Capabilities.HasFlag(StswSmtpCapability.AuthLogin))
		{
			await AuthenticateLoginAsync(username, password, cancellationToken).ConfigureAwait(false);
			return;
		}

		if (Capabilities.HasFlag(StswSmtpCapability.Authentication))
			throw new StswSmtpException("The SMTP server requires an authentication mechanism that is not supported by this client.", null, "AUTH");

		// Some servers do not advertise AUTH correctly, especially behind proxies.
		// LOGIN is a safe fallback because it is still one of the most widely supported basic SMTP mechanisms.
		await AuthenticateLoginAsync(username, password, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Sends a raw MIME message.
	/// </summary>
	/// <param name="from">The sender address.</param>
	/// <param name="recipients">The recipient addresses.</param>
	/// <param name="mimeMessage">The raw MIME message.</param>
	public void Send(string from, IEnumerable<string> recipients, string mimeMessage) => SendAsync(from, recipients, mimeMessage).GetAwaiter().GetResult();

	/// <summary>
	/// Sends a raw MIME message.
	/// </summary>
	/// <param name="from">The sender address.</param>
	/// <param name="recipients">The recipient addresses.</param>
	/// <param name="mimeMessage">The raw MIME message.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	public async Task SendAsync(string from, IEnumerable<string> recipients, string mimeMessage, CancellationToken cancellationToken = default)
	{
		ThrowIfDisposed();
		EnsureConnected();

		var sender = StswMailAddressParser.ExtractAddress(from);
		var recipientList = StswMailAddressParser.SplitAddresses(recipients)
			.Select(StswMailAddressParser.ExtractAddress)
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		if (recipientList.Count == 0)
			throw new ArgumentException("At least one recipient address is required.", nameof(recipients));

		await SendCommandAsync($"MAIL FROM:<{sender}>", [250], cancellationToken).ConfigureAwait(false);

		foreach (var recipient in recipientList)
			await SendCommandAsync($"RCPT TO:<{recipient}>", [250, 251], cancellationToken).ConfigureAwait(false);

		await SendCommandAsync("DATA", [354], cancellationToken).ConfigureAwait(false);
		await WriteDataAsync(mimeMessage, cancellationToken).ConfigureAwait(false);

		var response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
		EnsureResponse(response, "DATA body", 250);
	}

	/// <summary>
	/// Disconnects from the SMTP server.
	/// </summary>
	/// <param name="quit">Determines whether to send the QUIT command.</param>
	public void Disconnect(bool quit = true) => DisconnectAsync(quit).GetAwaiter().GetResult();

	/// <summary>
	/// Disconnects from the SMTP server.
	/// </summary>
	/// <param name="quit">Determines whether to send the QUIT command.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	public async Task DisconnectAsync(bool quit = true, CancellationToken cancellationToken = default)
	{
		if (!IsConnected)
		{
			Close();
			return;
		}

		if (quit)
		{
			try
			{
				await SendCommandAsync("QUIT", [221], cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				// Disconnect should be best-effort. The connection is closed below anyway.
			}
		}

		Close();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (_disposed)
			return;

		Close();
		_disposed = true;
	}

	private async Task AuthenticatePlainAsync(string username, string password, CancellationToken cancellationToken)
	{
		var token = Convert.ToBase64String(Encoding.UTF8.GetBytes("\0" + username + "\0" + password));
		await SendCommandAsync($"AUTH PLAIN {token}", [235], cancellationToken, "AUTH PLAIN ***").ConfigureAwait(false);
	}

	private async Task AuthenticateLoginAsync(string username, string password, CancellationToken cancellationToken)
	{
		await SendCommandAsync("AUTH LOGIN", [334], cancellationToken).ConfigureAwait(false);

		var usernameToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(username));
		await SendCommandAsync(usernameToken, [334], cancellationToken, "AUTH LOGIN <username>").ConfigureAwait(false);

		var passwordToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
		await SendCommandAsync(passwordToken, [235], cancellationToken, "AUTH LOGIN <password>").ConfigureAwait(false);
	}

	private async Task SendEhloOrHeloAsync(CancellationToken cancellationToken)
	{
		Capabilities = StswSmtpCapability.None;
		MaxMessageSize = null;

		var localDomain = string.IsNullOrWhiteSpace(LocalDomain) ? GetDefaultLocalDomain() : LocalDomain!;
		await WriteCommandAsync($"EHLO {localDomain}", cancellationToken).ConfigureAwait(false);

		var ehloResponse = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
		if (ehloResponse.StatusCode == 250)
		{
			ParseCapabilities(ehloResponse);
			return;
		}

		if (ehloResponse.StatusCode >= 500 && ehloResponse.StatusCode < 600)
		{
			await SendCommandAsync($"HELO {localDomain}", [250], cancellationToken).ConfigureAwait(false);
			return;
		}

		throw new StswSmtpException("Unexpected SMTP response.", ehloResponse, $"EHLO {localDomain}");
	}

	private void ParseCapabilities(StswSmtpResponse response)
	{
		Capabilities = StswSmtpCapability.None;
		MaxMessageSize = null;

		foreach (var line in response.Lines)
		{
			var text = line.Trim();
			if (string.IsNullOrWhiteSpace(text))
				continue;

			var splitIndex = text.IndexOfAny([' ', '\t']);
			var token = splitIndex >= 0 ? text.Substring(0, splitIndex) : text;
			var arguments = splitIndex >= 0 ? text.Substring(splitIndex + 1).Trim() : string.Empty;
			var normalizedToken = token.ToUpperInvariant();

			switch (normalizedToken)
			{
				case "STARTTLS":
					Capabilities |= StswSmtpCapability.StartTls;
					break;

				case "PIPELINING":
					Capabilities |= StswSmtpCapability.Pipelining;
					break;

				case "8BITMIME":
					Capabilities |= StswSmtpCapability.EightBitMime;
					break;

				case "SMTPUTF8":
					Capabilities |= StswSmtpCapability.SmtpUtf8;
					break;

				case "SIZE":
					Capabilities |= StswSmtpCapability.Size;
					if (long.TryParse(arguments, out var size))
						MaxMessageSize = size;
					break;

				case "AUTH":
					ParseAuthCapabilities(arguments);
					break;

				default:
					if (normalizedToken.StartsWith("AUTH=", StringComparison.OrdinalIgnoreCase))
					{
						var authArguments = normalizedToken.Substring("AUTH=".Length);
						if (!string.IsNullOrWhiteSpace(arguments))
							authArguments += " " + arguments;

						ParseAuthCapabilities(authArguments);
					}
					break;
			}
		}
	}

	private void ParseAuthCapabilities(string authArguments)
	{
		Capabilities |= StswSmtpCapability.Authentication;

		if (authArguments.IndexOf("PLAIN", StringComparison.OrdinalIgnoreCase) >= 0)
			Capabilities |= StswSmtpCapability.AuthPlain;

		if (authArguments.IndexOf("LOGIN", StringComparison.OrdinalIgnoreCase) >= 0)
			Capabilities |= StswSmtpCapability.AuthLogin;
	}

	private async Task SendCommandAsync(string command, int[] expectedStatusCodes, CancellationToken cancellationToken, string? displayCommand = null)
	{
		await WriteCommandAsync(command, cancellationToken).ConfigureAwait(false);

		var response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
		EnsureResponse(response, displayCommand ?? command, expectedStatusCodes);
	}

	private async Task WriteCommandAsync(string command, CancellationToken cancellationToken)
	{
		EnsureConnected();
		cancellationToken.ThrowIfCancellationRequested();

		var bytes = Encoding.ASCII.GetBytes(command + "\r\n");
		await _stream!.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
		await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	private async Task WriteDataAsync(string mimeMessage, CancellationToken cancellationToken)
	{
		EnsureConnected();
		cancellationToken.ThrowIfCancellationRequested();

		var normalized = StswMailEncoding.NormalizeCrlf(mimeMessage);
		var builder = new StringBuilder();

		using (var reader = new StringReader(normalized))
		{
			string? line;
			while ((line = reader.ReadLine()) is not null)
			{
				if (line.StartsWith(".", StringComparison.Ordinal))
					builder.Append('.');

				builder.Append(line).Append("\r\n");
			}
		}

		builder.Append(".\r\n");

		var bytes = Encoding.UTF8.GetBytes(builder.ToString());
		await _stream!.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
		await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	private async Task<StswSmtpResponse> ReadResponseAsync(CancellationToken cancellationToken)
	{
		EnsureConnected();
		cancellationToken.ThrowIfCancellationRequested();

		var lines = new List<string>();
		int? statusCode = null;

		while (true)
		{
			var rawLine = await _reader!.ReadLineAsync().ConfigureAwait(false) ?? throw new StswSmtpException("The SMTP server closed the connection unexpectedly.");
			if (rawLine.Length < 3 || !int.TryParse(rawLine.Substring(0, 3), out var currentStatusCode))
				throw new StswSmtpException($"Invalid SMTP response: {rawLine}");

			statusCode ??= currentStatusCode;

			var separator = rawLine.Length > 3 ? rawLine[3] : ' ';
			var text = rawLine.Length > 4 ? rawLine.Substring(4) : string.Empty;
			lines.Add(text);

			if (separator != '-')
				break;
		}

		return new StswSmtpResponse(statusCode.GetValueOrDefault(), lines);
	}

	private async Task UpgradeToTlsAsync(string host, CancellationToken cancellationToken)
	{
		EnsureConnected();
		cancellationToken.ThrowIfCancellationRequested();

		var sslStream = new SslStream(_stream!, false, ValidateServerCertificate);
		await sslStream.AuthenticateAsClientAsync(host).ConfigureAwait(false);

		_stream = sslStream;
		IsSecure = true;
		ResetReader();
	}

	private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
	{
		if (ServerCertificateValidationCallback is not null)
			return ServerCertificateValidationCallback(sender, certificate, chain, sslPolicyErrors);

		return sslPolicyErrors == SslPolicyErrors.None;
	}

	private void ResetReader()
	{
		EnsureStream();
		_reader = new StreamReader(_stream!, Encoding.ASCII, false, 1024, true);
	}

	private static void EnsureResponse(StswSmtpResponse response, string command, params int[] expectedStatusCodes)
	{
		if (expectedStatusCodes.Contains(response.StatusCode))
			return;

		throw new StswSmtpException("Unexpected SMTP response.", response, command);
	}

	private static StswMailSecurityOption ResolveSecurityOption(int port, StswMailSecurityOption securityOption)
	{
		if (securityOption != StswMailSecurityOption.Auto)
			return securityOption;

		return port switch
		{
			465 => StswMailSecurityOption.SslOnConnect,
			587 => StswMailSecurityOption.StartTls,
			25 => StswMailSecurityOption.StartTlsWhenAvailable,
			_ => StswMailSecurityOption.StartTlsWhenAvailable,
		};
	}

	private static string GetDefaultLocalDomain()
	{
		try
		{
			var hostName = Dns.GetHostName();
			return string.IsNullOrWhiteSpace(hostName) ? "localhost" : hostName;
		}
		catch
		{
			return "localhost";
		}
	}

	private void EnsureConnected()
	{
		ThrowIfDisposed();

		if (!IsConnected || _reader is null)
			throw new InvalidOperationException("The SMTP client is not connected.");
	}

	private void EnsureStream()
	{
		if (_stream is null)
			throw new InvalidOperationException("The SMTP stream is not available.");
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(StswSmtpClient));
	}

	private void Close()
	{
		try { _reader?.Dispose(); } catch { }
		try { _stream?.Dispose(); } catch { }
		try { _tcpClient?.Close(); } catch { }

		_reader = null;
		_stream = null;
		_tcpClient = null;
		Capabilities = StswSmtpCapability.None;
		MaxMessageSize = null;
		IsSecure = false;
	}
}
