using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Provides helper methods for parsing and formatting mail addresses.
/// </summary>
public static class StswMailAddressParser
{
	/// <summary>
	/// Splits a collection of mail address strings into individual addresses.
	/// </summary>
	/// <param name="values">The address strings to split.</param>
	/// <returns>The normalized address strings.</returns>
	public static IEnumerable<string> SplitAddresses(IEnumerable<string>? values)
	{
		if (values is null)
			yield break;

		foreach (var value in values)
			foreach (var address in SplitAddresses(value))
				if (!string.IsNullOrWhiteSpace(address))
					yield return address.Trim();
	}

	/// <summary>
	/// Extracts the email address part from a formatted address.
	/// </summary>
	/// <param name="value">The address string.</param>
	/// <returns>The email address part.</returns>
	public static string ExtractAddress(string value)
	{
		var (_, address) = Parse(value);
		return address;
	}

	/// <summary>
	/// Formats an address for a MIME header.
	/// </summary>
	/// <param name="value">The address string.</param>
	/// <returns>The MIME formatted address.</returns>
	public static string FormatAddress(string value)
	{
		var (displayName, address) = Parse(value);
		if (string.IsNullOrWhiteSpace(displayName))
			return address;

		return $"{StswMailEncoding.EncodeHeader(displayName)} <{address}>";
	}

	/// <summary>
	/// Parses a mail address into display name and address parts.
	/// </summary>
	/// <param name="value">The address string.</param>
	/// <returns>The parsed display name and address.</returns>
	public static (string? DisplayName, string Address) Parse(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new FormatException("Email address cannot be empty.");

		value = value.Trim();

		var start = value.LastIndexOf('<');
		var end = value.LastIndexOf('>');
		if (start >= 0 && end > start)
		{
			var displayName = value.Substring(0, start).Trim().Trim('"');
			var address = value.Substring(start + 1, end - start - 1).Trim();
			ValidateAddress(address);
			return (string.IsNullOrWhiteSpace(displayName) ? null : displayName, address);
		}

		ValidateAddress(value);
		return (null, value);
	}

	private static IEnumerable<string> SplitAddresses(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			yield break;

		var current = new StringBuilder();
		var inQuotes = false;
		var angleDepth = 0;

		foreach (var ch in value)
		{
			if (ch == '"')
				inQuotes = !inQuotes;

			if (!inQuotes)
			{
				if (ch == '<')
					angleDepth++;
				else if (ch == '>' && angleDepth > 0)
					angleDepth--;

				if ((ch == ',' || ch == ';') && angleDepth == 0)
				{
					yield return current.ToString();
					current.Clear();
					continue;
				}
			}

			current.Append(ch);
		}

		if (current.Length > 0)
			yield return current.ToString();
	}

	private static void ValidateAddress(string address)
	{
		if (string.IsNullOrWhiteSpace(address) || !address.Contains('@'))
			throw new FormatException($"Invalid email address: {address}");

		if (address.Any(char.IsWhiteSpace))
			throw new FormatException($"Invalid email address: {address}");
	}
}
