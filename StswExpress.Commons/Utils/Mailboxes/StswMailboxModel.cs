using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace StswExpress.Commons;
/// <summary>
/// Represents an email account and provides methods for sending emails using the account's SMTP settings.
/// </summary>
[StswInfo(null, Changes = StswPlannedChanges.ChangeName)]
public partial class StswMailboxModel : StswObservableObject
{
    /*
    private static int _emailsSentToday = 0;
    private static int _emailsSentThisHour = 0;
    private static DateTime _lastEmailSentDate = DateTime.MinValue;
    private static DateTime _lastEmailSentHour = DateTime.MinValue;
    */

    public StswMailboxModel() { }
    public StswMailboxModel(string? host = null, string? from = null, string? username = null, string? password = null) : this(host, null, from, username, password) { }
    public StswMailboxModel(string? host = null, int? port = null, string? from = null, string? username = null, string? password = null)
    {
        Host = host;
        Port = port;
        From = from;
        Username = username;
        Password = password;
    }

    /// <summary>
    /// Gets or sets the name of the email account.
    /// </summary>
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string? _name;

    /// <summary>
    /// Gets or sets the SMTP host for the email account.
    /// </summary>
    public string? Host
    {
        get => _host;
        set => SetProperty(ref _host, value);
    }
    private string? _host;

    /// <summary>
    /// Gets or sets the port number for the SMTP host.
    /// </summary>
    public int? Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }
    private int? _port = 587;

    /// <summary>
    /// Gets or sets the email address associated with the email account.
    /// </summary>
    public string? From
    {
        get => _from;
        set => SetProperty(ref _from, value);
    }
    private string? _from;

    /// <summary>
    /// Gets or sets the username for the email account.
    /// </summary>
    public string? Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }
    private string? _username;

    /// <summary>
    /// Gets or sets the password for the email account.
    /// </summary>
    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    private string? _password;

    /// <summary>
    /// Gets or sets the domain for the email account.
    /// </summary>
    [StswInfo("0.8.2")]
    public string? Domain
    {
        get => _domain;
        set => SetProperty(ref _domain, value);
    }
    private string? _domain;

    /// <summary>
    /// Gets or sets the collection of reply-to addresses for the email account.
    /// </summary>
    [StswInfo("0.9.0")]
    public IEnumerable<string>? ReplyTo
    {
        get => _replyTo;
        set => SetProperty(ref _replyTo, value);
    }
    private IEnumerable<string>? _replyTo;

    /// <summary>
    /// Gets or sets a value indicating whether to ignore SSL certificate errors when connecting to the SMTP server.
    /// </summary>
    [StswInfo("0.19.1")]
    public bool IgnoreCertificateErrors
    {
        get => _ignoreCertificateErrors;
        set => SetProperty(ref _ignoreCertificateErrors, value);
    }
    private bool _ignoreCertificateErrors;

    /// <summary>
    /// Gets or sets the security option for the SMTP connection.
    /// </summary>
    [StswInfo("0.19.0")]
    public SecureSocketOptions SecurityOption
    {
        get => _securityOption;
        set => SetProperty(ref _securityOption, value);
    }
    private SecureSocketOptions _securityOption = SecureSocketOptions.Auto;

    /// <summary>
    /// Sends an email using the SMTP protocol with optional attachments, BCC recipients, and reply-to addresses.
    /// Automatically redirects emails to the configured debug recipient if the application is running in DEBUG mode.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    /// <param name="bcc">An optional collection of BCC recipients.</param>
    public void Send(IEnumerable<string> to, string subject, string body, bool? isBodyHtml = null, IEnumerable<string>? attachments = null, IEnumerable<string>? cc = null, IEnumerable<string>? bcc = null)
    {
        if (!StswMailboxes.Config.IsEnabled)
            return;

        // if (!CanSendEmail())
        //     return;

        ArgumentException.ThrowIfNullOrEmpty(Host);
        ArgumentException.ThrowIfNullOrEmpty(From);
        ArgumentNullException.ThrowIfNull(Port);

        var message = BuildMessage(to, subject, body, isBodyHtml ?? false, attachments, cc, bcc);

        using var client = CreateConfiguredClient();
        client.Connect(Host, Port.Value, SecurityOption);
        if (!string.IsNullOrEmpty(Username))
            client.Authenticate(Username, Password);

        client.Send(message);
        client.Disconnect(true);

        // IncrementEmailCounters();
    }

    /// <summary>
    /// Sends an email using the SMTP protocol with optional attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    public void Send(IEnumerable<string> to, string subject, string body, IEnumerable<string> attachments)
        => Send(to, subject, body, null, attachments, null);

    /// <summary>
    /// Sends an email using the SMTP protocol without attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    public void Send(IEnumerable<string> to, string subject, string body)
        => Send(to, subject, body, null, null, null);

    /// <summary>
    /// Asynchronously sends an email using the SMTP protocol with optional attachments, BCC recipients, and reply-to addresses.
    /// Automatically redirects emails to the configured debug recipient if the application is running in DEBUG mode.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    /// <param name="bcc">An optional collection of BCC recipients.</param>
    public async Task SendAsync(IEnumerable<string> to, string subject, string body, bool? isBodyHtml = null, IEnumerable<string>? attachments = null, IEnumerable<string>? cc = null, IEnumerable<string>? bcc = null)
    {
        if (!StswMailboxes.Config.IsEnabled)
            return;

        // if (!CanSendEmail())
        //     return;

        ArgumentException.ThrowIfNullOrEmpty(Host);
        ArgumentException.ThrowIfNullOrEmpty(From);
        ArgumentNullException.ThrowIfNull(Port);

        var message = BuildMessage(to, subject, body, isBodyHtml ?? false, attachments, cc, bcc);

        using var client = CreateConfiguredClient();
        client.LocalDomain = Domain;
        await client.ConnectAsync(Host, Port.Value, SecurityOption);
        if (!string.IsNullOrEmpty(Username))
            await client.AuthenticateAsync(Username, Password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        // IncrementEmailCounters();
    }

    /// <summary>
    /// Sends an email using the SMTP protocol with optional attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    public Task SendAsync(IEnumerable<string> to, string subject, string body, IEnumerable<string> attachments)
        => SendAsync(to, subject, body, null, attachments, null);

    /// <summary>
    /// Sends an email using the SMTP protocol without attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    public Task SendAsync(IEnumerable<string> to, string subject, string body)
        => SendAsync(to, subject, body, null, null, null);

    /// <summary>
    /// Builds a <see cref="MimeMessage"/> for sending an email with optional attachments, BCC recipients, and reply-to addresses.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="isBodyHtml">Indicates whether the body content is in HTML format.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    /// <param name="cc">The collection of CC recipients.</param>
    /// <param name="bcc">The collection of BCC recipients.</param>
    /// <returns>A configured <see cref="MimeMessage"/> instance.</returns>
    [StswInfo("0.19.1")]
    private MimeMessage BuildMessage(IEnumerable<string> to, string subject, string body, bool isBodyHtml, IEnumerable<string>? attachments = null, IEnumerable<string>? cc = null, IEnumerable<string>? bcc = null)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(From));

        if (StswFn.IsInDebug && !StswMailboxes.Config.DebugEmailRecipients.IsNullOrEmpty())
        {
            foreach (var email in StswMailboxes.Config.DebugEmailRecipients!)
                if (!string.IsNullOrEmpty(email))
                    message.To.Add(MailboxAddress.Parse(email));
        }
        else
        {
            to?.ForEach(x => message.To.Add(MailboxAddress.Parse(x)));
            cc?.ForEach(x => message.Cc.Add(MailboxAddress.Parse(x)));
            bcc?.ForEach(x => message.Bcc.Add(MailboxAddress.Parse(x)));
            ReplyTo?.ForEach(x => message.ReplyTo.Add(MailboxAddress.Parse(x)));
        }

        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = isBodyHtml ? body : null,
            TextBody = isBodyHtml ? null : body
        };

        attachments?.ForEach(file => builder.Attachments.Add(file));
        message.Body = builder.ToMessageBody();

        return message;
    }

    /// <summary>
    /// Creates a configured <see cref="SmtpClient"/> instance for sending emails.
    /// </summary>
    /// <returns>A configured <see cref="SmtpClient"/> instance.</returns>
    [StswInfo("0.19.1")]
    private SmtpClient CreateConfiguredClient()
    {
        var client = new SmtpClient();

        if (IgnoreCertificateErrors)
            client.ServerCertificateValidationCallback = (s, c, chain, sslPolicyErrors) => true;

        return client;
    }

    /*
    private bool CanSendEmail()
    {
        var now = DateTime.Now;

        // Reset counters if a new day has started
        if (_lastEmailSentDate.Date != now.Date)
        {
            _emailsSentToday = 0;
            _lastEmailSentDate = now;
        }

        // Reset hourly counter if a new hour has started
        if (_lastEmailSentHour.Hour != now.Hour)
        {
            _emailsSentThisHour = 0;
            _lastEmailSentHour = now;
        }

        // Check daily limit
        if (StswMailboxex.Config.MaxEmailsPerDay > 0 && _emailsSentToday >= StswMailboxex.Config.MaxEmailsPerDay)
        {
            Console.WriteLine("Daily email limit reached.");
            return false;
        }

        // Check hourly limit
        if (StswMailboxex.Config.MaxEmailsPerHour > 0 && _emailsSentThisHour >= StswMailboxex.Config.MaxEmailsPerHour)
        {
            Console.WriteLine("Hourly email limit reached.");
            return false;
        }

        return true;
    }

    private void IncrementEmailCounters()
    {
        _emailsSentToday++;
        _emailsSentThisHour++;
    }
    */
}
