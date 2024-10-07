using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System;
using System.Threading.Tasks;

namespace StswExpress;
/// <summary>
/// Represents an email account and provides methods for sending emails using the account's SMTP settings.
/// </summary>
public class StswMailboxModel : StswObservableObject
{
    /*
    private static int _emailsSentToday = 0;
    private static int _emailsSentThisHour = 0;
    private static DateTime _lastEmailSentDate = DateTime.MinValue;
    private static DateTime _lastEmailSentHour = DateTime.MinValue;
    */

    public StswMailboxModel() { }
    public StswMailboxModel(string? host = null, string? address = null, string? username = null, string? password = null)
    {
        Host = host;
        Address = address;
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
    private int? _port;

    /// <summary>
    /// Gets or sets the email address associated with the email account.
    /// </summary>
    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }
    private string? _address;

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
    public string? Domain
    {
        get => _domain;
        set => SetProperty(ref _domain, value);
    }
    private string? _domain;

    /// <summary>
    /// Gets or sets a value indicating whether SSL is enabled for the email account.
    /// </summary>
    public bool EnableSSL
    {
        get => _enableSSL;
        set => SetProperty(ref _enableSSL, value);
    }
    private bool _enableSSL = false;

    /// <summary>
    /// Gets or sets the collection of BCC recipients for the email account.
    /// </summary>
    public IEnumerable<string>? BCC
    {
        get => _bcc;
        set => SetProperty(ref _bcc, value);
    }
    private IEnumerable<string>? _bcc;

    /// <summary>
    /// Gets or sets the collection of reply-to addresses for the email account.
    /// </summary>
    public IEnumerable<string>? ReplyTo
    {
        get => _replyTo;
        set => SetProperty(ref _replyTo, value);
    }
    private IEnumerable<string>? _replyTo;

    /// <summary>
    /// Sends an email using the SMTP protocol with optional attachments, BCC recipients, and reply-to addresses.
    /// Automatically redirects emails to the configured debug recipient if the application is running in DEBUG mode.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    /// <param name="bcc">An optional collection of BCC recipients.</param>
    /// <param name="reply">An optional collection of reply-to addresses.</param>
    public void Send(IEnumerable<string> to, string subject, string body, IEnumerable<string>? attachments = null, IEnumerable<string>? bcc = null, IEnumerable<string>? reply = null)
    {
        if (!StswMailboxes.Config.IsEnabled)
            return;

        // if (!CanSendEmail())
        //     return;

        ArgumentException.ThrowIfNullOrEmpty(Address);
        ArgumentNullException.ThrowIfNull(Port);

        using var mail = new MailMessage();
        mail.From = new MailAddress(Address);
        mail.Subject = subject;
        mail.Body = body;

        if (StswFn.IsInDebug() && !string.IsNullOrEmpty(StswMailboxes.Config.DebugEmailRecipient))
        {
            AddRecipients([StswMailboxes.Config.DebugEmailRecipient], mail.To.Add);
        }
        else
        {
            AddRecipients(to, mail.To.Add);
            AddRecipients(bcc, mail.Bcc.Add);
            AddRecipients(reply, mail.ReplyToList.Add);
        }
        AddRecipients(attachments, x => mail.Attachments.Add(new Attachment(x)));

        using var smtp = new SmtpClient(Host, Port.Value)
        {
            Credentials = new NetworkCredential(Username, Password, Domain),
            EnableSsl = EnableSSL
        };
        smtp.Send(mail);

        // IncrementEmailCounters();
    }

    /// <summary>
    /// Sends an email using the SMTP protocol with optional attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    public void Send(IEnumerable<string> to, string subject, string body, IEnumerable<string> attachments) => Send(to, subject, body, attachments, BCC, ReplyTo);

    /// <summary>
    /// Sends an email using the SMTP protocol without attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    public void Send(IEnumerable<string> to, string subject, string body) => Send(to, subject, body, null, BCC, ReplyTo);

    /// <summary>
    /// Asynchronously sends an email using the SMTP protocol with optional attachments, BCC recipients, and reply-to addresses.
    /// Automatically redirects emails to the configured debug recipient if the application is running in DEBUG mode.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    /// <param name="bcc">An optional collection of BCC recipients.</param>
    /// <param name="reply">An optional collection of reply-to addresses.</param>
    public async Task SendAsync(IEnumerable<string> to, string subject, string body, IEnumerable<string>? attachments = null, IEnumerable<string>? bcc = null, IEnumerable<string>? reply = null)
    {
        if (!StswMailboxes.Config.IsEnabled)
            return;

        // if (!CanSendEmail())
        //     return;

        ArgumentException.ThrowIfNullOrEmpty(Address);
        ArgumentNullException.ThrowIfNull(Port);

        using var mail = new MailMessage();
        mail.From = new MailAddress(Address);
        mail.Subject = subject;
        mail.Body = body;

        if (StswFn.IsInDebug() && !string.IsNullOrEmpty(StswMailboxes.Config.DebugEmailRecipient))
        {
            AddRecipients([StswMailboxes.Config.DebugEmailRecipient], mail.To.Add);
        }
        else
        {
            AddRecipients(to, mail.To.Add);
            AddRecipients(bcc, mail.Bcc.Add);
            AddRecipients(reply, mail.ReplyToList.Add);
        }
        AddRecipients(attachments, x => mail.Attachments.Add(new Attachment(x)));

        using var smtp = new SmtpClient(Host, Port.Value)
        {
            Credentials = new NetworkCredential(Username, Password, Domain),
            EnableSsl = EnableSSL
        };
        await smtp.SendMailAsync(mail);

        // IncrementEmailCounters();
    }

    /// <summary>
    /// Sends an email using the SMTP protocol with optional attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    public Task SendAsync(IEnumerable<string> to, string subject, string body, IEnumerable<string> attachments) => SendAsync(to, subject, body, attachments, BCC, ReplyTo);

    /// <summary>
    /// Sends an email using the SMTP protocol without attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    public Task SendAsync(IEnumerable<string> to, string subject, string body) => SendAsync(to, subject, body, null, BCC, ReplyTo);

    /// <summary>
    /// Adds a collection of recipients to a specified mail collection, using the provided action to add each recipient.
    /// </summary>
    /// <param name="recipients">The collection of recipient addresses to be added. If null, no recipients will be added.</param>
    /// <param name="addAction">The action used to add each recipient to the mail collection (e.g., To, Bcc, or ReplyTo list).</param>
    private void AddRecipients(IEnumerable<string>? recipients, Action<string> addAction)
    {
        if (recipients != null)
            foreach (var recipient in recipients)
                addAction(recipient);
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
