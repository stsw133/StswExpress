using System.Globalization;
using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Provides helper methods for encoding mail headers, body parts, and MIME parameters.
/// </summary>
public static class StswMailEncoding
{
	/// <summary>
	/// Encodes a mail header value using RFC 2047 encoded-word syntax when needed.
	/// </summary>
	/// <param name="value">The header value to encode.</param>
	/// <returns>The encoded header value.</returns>
	public static string EncodeHeader(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		if (IsAsciiHeaderSafe(value))
			return value;

		var bytes = Encoding.UTF8.GetBytes(value);
		return $"=?utf-8?B?{Convert.ToBase64String(bytes)}?=";
	}

	/// <summary>
	/// Encodes text using quoted-printable transfer encoding.
	/// </summary>
	/// <param name="value">The text to encode.</param>
	/// <returns>The quoted-printable encoded text.</returns>
	public static string EncodeQuotedPrintable(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		var normalized = NormalizeNewLines(value);
		var result = new StringBuilder();
		var lineLength = 0;
		var lines = normalized.Split('\n');

		for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
		{
			var line = lines[lineIndex];
			if (line.EndsWith("\r", StringComparison.Ordinal))
				line = line.Substring(0, line.Length - 1);

			var bytes = Encoding.UTF8.GetBytes(line);
			for (var i = 0; i < bytes.Length; i++)
			{
				var b = bytes[i];
				var isLastByteInLine = i == bytes.Length - 1;
				var token = EncodeQuotedPrintableByte(b, isLastByteInLine);

				if (lineLength + token.Length > 75)
				{
					result.Append("=\r\n");
					lineLength = 0;
				}

				result.Append(token);
				lineLength += token.Length;
			}

			if (lineIndex < lines.Length - 1)
			{
				result.Append("\r\n");
				lineLength = 0;
			}
		}

		return result.ToString();
	}

	/// <summary>
	/// Encodes binary data as Base64 with MIME line wrapping.
	/// </summary>
	/// <param name="data">The binary data to encode.</param>
	/// <returns>The Base64 encoded text.</returns>
	public static string EncodeBase64Lines(byte[] data)
	{
		if (data.Length == 0)
			return string.Empty;

		var base64 = Convert.ToBase64String(data);
		var result = new StringBuilder();

		for (var i = 0; i < base64.Length; i += 76)
		{
			if (i > 0)
				result.Append("\r\n");

			result.Append(base64.Substring(i, Math.Min(76, base64.Length - i)));
		}

		return result.ToString();
	}

	/// <summary>
	/// Encodes a MIME parameter value using RFC 2231 percent encoding.
	/// </summary>
	/// <param name="value">The parameter value to encode.</param>
	/// <returns>The encoded parameter value.</returns>
	public static string EncodeParameter(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		var bytes = Encoding.UTF8.GetBytes(value);
		var result = new StringBuilder("utf-8''");

		foreach (var b in bytes)
		{
			var ch = (char)b;
			if (IsAttributeChar(ch))
				result.Append(ch);
			else
				result.Append('%').Append(b.ToString("X2", CultureInfo.InvariantCulture));
		}

		return result.ToString();
	}

	/// <summary>
	/// Normalizes all line endings to CRLF.
	/// </summary>
	/// <param name="value">The text to normalize.</param>
	/// <returns>The normalized text.</returns>
	public static string NormalizeCrlf(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		return NormalizeNewLines(value).Replace("\n", "\r\n");
	}

	private static string NormalizeNewLines(string value) => value.Replace("\r\n", "\n").Replace("\r", "\n");

	private static string EncodeQuotedPrintableByte(byte b, bool isLastByteInLine)
	{
		if (b == 32 && !isLastByteInLine)
			return " ";

		if (b == 9 && !isLastByteInLine)
			return "\t";

		if (b >= 33 && b <= 60 || b >= 62 && b <= 126)
			return ((char)b).ToString();

		return "=" + b.ToString("X2", CultureInfo.InvariantCulture);
	}

	private static bool IsAsciiHeaderSafe(string value)
		=> value.All(x => x >= 32 && x <= 126 && x != '=' && x != '?' && x != '_');

	private static bool IsAttributeChar(char ch)
		=> ch >= 'A' && ch <= 'Z'
		|| ch >= 'a' && ch <= 'z'
		|| ch >= '0' && ch <= '9'
		|| ch is '!' or '#' or '$' or '&' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~';
}
