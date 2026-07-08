using System.Globalization;
using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Writes MIME messages for SMTP delivery.
/// </summary>
public static class StswMimeWriter
{
	/// <summary>
	/// Builds a MIME message.
	/// </summary>
	/// <param name="from">The sender address.</param>
	/// <param name="to">The recipient addresses.</param>
	/// <param name="subject">The message subject.</param>
	/// <param name="body">The message body.</param>
	/// <param name="isBodyHtml">Determines whether the body is HTML.</param>
	/// <param name="attachments">The optional attachment file paths.</param>
	/// <param name="cc">The optional CC recipient addresses.</param>
	/// <param name="replyTo">The optional reply-to addresses.</param>
	/// <returns>The raw MIME message.</returns>
	public static string Write(
		string from,
		IEnumerable<string> to,
		string subject,
		string body,
		bool isBodyHtml = false,
		IEnumerable<string>? attachments = null,
		IEnumerable<string>? cc = null,
		IEnumerable<string>? replyTo = null)
	{
		var normalizedTo = NormalizeAddresses(to).ToList();
		var normalizedCc = NormalizeAddresses(cc).ToList();
		var normalizedReplyTo = NormalizeAddresses(replyTo).ToList();
		var normalizedAttachments = attachments?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? [];

		if (string.IsNullOrWhiteSpace(from))
			throw new ArgumentException("Sender address cannot be empty.", nameof(from));

		if (normalizedTo.Count == 0)
			throw new ArgumentException("At least one recipient address is required.", nameof(to));

		var builder = new StringBuilder();

		AppendHeader(builder, "Date", DateTimeOffset.UtcNow.ToString("r", CultureInfo.InvariantCulture));
		AppendHeader(builder, "Message-ID", CreateMessageId(from));
		AppendHeader(builder, "From", StswMailAddressParser.FormatAddress(from));
		AppendHeader(builder, "To", string.Join(", ", normalizedTo.Select(StswMailAddressParser.FormatAddress)));

		if (normalizedCc.Count > 0)
			AppendHeader(builder, "Cc", string.Join(", ", normalizedCc.Select(StswMailAddressParser.FormatAddress)));

		if (normalizedReplyTo.Count > 0)
			AppendHeader(builder, "Reply-To", string.Join(", ", normalizedReplyTo.Select(StswMailAddressParser.FormatAddress)));

		AppendHeader(builder, "Subject", StswMailEncoding.EncodeHeader(SanitizeHeaderValue(subject)));
		AppendHeader(builder, "MIME-Version", "1.0");

		if (normalizedAttachments.Count == 0)
		{
			AppendBodyPartHeaders(builder, isBodyHtml);
			builder.Append("\r\n");
			builder.Append(StswMailEncoding.EncodeQuotedPrintable(body));
			builder.Append("\r\n");
			return builder.ToString();
		}

		var boundary = CreateBoundary();
		AppendHeader(builder, "Content-Type", $"multipart/mixed; boundary=\"{boundary}\"");
		builder.Append("\r\n");
		builder.Append("This is a multi-part message in MIME format.\r\n");
		builder.Append("\r\n");

		AppendBodyPart(builder, boundary, body, isBodyHtml);

		foreach (var attachment in normalizedAttachments)
			AppendAttachmentPart(builder, boundary, attachment);

		builder.Append("--").Append(boundary).Append("--\r\n");

		return builder.ToString();
	}

	private static IEnumerable<string> NormalizeAddresses(IEnumerable<string>? addresses) => StswMailAddressParser.SplitAddresses(addresses);

	private static void AppendHeader(StringBuilder builder, string name, string value)
	{
		builder.Append(name)
			.Append(": ")
			.Append(SanitizeHeaderValue(value))
			.Append("\r\n");
	}

	private static void AppendBodyPart(StringBuilder builder, string boundary, string body, bool isBodyHtml)
	{
		builder.Append("--").Append(boundary).Append("\r\n");
		AppendBodyPartHeaders(builder, isBodyHtml);
		builder.Append("\r\n");
		builder.Append(StswMailEncoding.EncodeQuotedPrintable(body));
		builder.Append("\r\n");
	}

	private static void AppendBodyPartHeaders(StringBuilder builder, bool isBodyHtml)
	{
		AppendHeader(builder, "Content-Type", $"{(isBodyHtml ? "text/html" : "text/plain")}; charset=utf-8");
		AppendHeader(builder, "Content-Transfer-Encoding", "quoted-printable");
	}

	private static void AppendAttachmentPart(StringBuilder builder, string boundary, string filePath)
	{
		if (!File.Exists(filePath))
			throw new FileNotFoundException("Attachment file was not found.", filePath);

		var fileName = Path.GetFileName(filePath);
		var contentType = GetContentType(filePath);
		var encodedFileName = StswMailEncoding.EncodeParameter(fileName);
		var bytes = File.ReadAllBytes(filePath);

		builder.Append("--").Append(boundary).Append("\r\n");
		AppendHeader(builder, "Content-Type", $"{contentType}; name*={encodedFileName}");
		AppendHeader(builder, "Content-Transfer-Encoding", "base64");
		AppendHeader(builder, "Content-Disposition", $"attachment; filename*={encodedFileName}");
		builder.Append("\r\n");
		builder.Append(StswMailEncoding.EncodeBase64Lines(bytes));
		builder.Append("\r\n");
	}

	private static string CreateBoundary() => "----=_StswExpress_" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

	private static string CreateMessageId(string from)
	{
		var address = StswMailAddressParser.ExtractAddress(from);
		var atIndex = address.LastIndexOf('@');
		var domain = atIndex >= 0 && atIndex < address.Length - 1
			? address.Substring(atIndex + 1)
			: "localhost";

		return $"<{Guid.NewGuid():N}@{domain}>";
	}

	private static string SanitizeHeaderValue(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		return value.Replace("\r", " ").Replace("\n", " ").Trim();
	}

	private static string GetContentType(string filePath) => Path.GetExtension(filePath).ToLowerInvariant() switch
	{
		".txt" => "text/plain",
		".csv" => "text/csv",
		".htm" => "text/html",
		".html" => "text/html",
		".xml" => "application/xml",
		".json" => "application/json",
		".pdf" => "application/pdf",
		".zip" => "application/zip",
		".7z" => "application/x-7z-compressed",
		".rar" => "application/vnd.rar",
		".gz" => "application/gzip",
		".jpg" => "image/jpeg",
		".jpeg" => "image/jpeg",
		".png" => "image/png",
		".gif" => "image/gif",
		".bmp" => "image/bmp",
		".webp" => "image/webp",
		".svg" => "image/svg+xml",
		".doc" => "application/msword",
		".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
		".xls" => "application/vnd.ms-excel",
		".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
		".ppt" => "application/vnd.ms-powerpoint",
		".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
		_ => "application/octet-stream",
	};
}
