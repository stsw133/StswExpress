using System.Collections.ObjectModel;

namespace StswExpress.Commons;

/// <summary>
/// Represents an SMTP server response.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="StswSmtpResponse"/> class.
/// </remarks>
/// <param name="statusCode">The SMTP status code.</param>
/// <param name="lines">The response text lines.</param>
public sealed class StswSmtpResponse(int statusCode, IEnumerable<string> lines)
{
	/// <summary>
	/// Gets the SMTP status code.
	/// </summary>
	public int StatusCode { get; } = statusCode;

	/// <summary>
	/// Gets the response text lines without the status code prefix.
	/// </summary>
	public IReadOnlyList<string> Lines { get; } = new ReadOnlyCollection<string>((lines ?? Enumerable.Empty<string>()).ToList());

	/// <summary>
	/// Gets the response text.
	/// </summary>
	public string Text => string.Join(Environment.NewLine, Lines);

	/// <summary>
	/// Gets a value indicating whether the response is positive.
	/// </summary>
	public bool IsSuccess => StatusCode >= 200 && StatusCode < 400;

	/// <summary>
	/// Gets a value indicating whether the response is a positive completion response.
	/// </summary>
	public bool IsPositiveCompletion => StatusCode >= 200 && StatusCode < 300;

	/// <summary>
	/// Gets a value indicating whether the response is a positive intermediate response.
	/// </summary>
	public bool IsPositiveIntermediate => StatusCode >= 300 && StatusCode < 400;

	/// <inheritdoc/>
	public override string ToString() => string.IsNullOrWhiteSpace(Text) ? StatusCode.ToString() : $"{StatusCode} {Text}";
}
