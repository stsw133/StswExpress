using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System;

namespace StswExpress;
/// <summary>
/// Represents an email account and provides a way to send emails using that account.
/// </summary>
public class StswMailboxModel : StswObservableObject
{
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
    /// Sends an email using the SMTP protocol.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="attachments">An optional collection of file paths to attach to the email.</param>
    /// <param name="bcc">An optional collection of BCC recipients.</param>
    /// <param name="reply">An optional collection of reply-to addresses.</param>
    public void Send(IEnumerable<string> to, string subject, string body, IEnumerable<string>? attachments = null, IEnumerable<string>? bcc = null, IEnumerable<string>? reply = null)
    {
        if (string.IsNullOrEmpty(Address))
            throw new ArgumentNullException(nameof(Address));
        if (Port == null)
            throw new ArgumentNullException(nameof(Port));

        using var mail = new MailMessage();
        mail.From = new MailAddress(Address);
        mail.Subject = subject;
        mail.Body = body;

        foreach (var x in to)
            mail.To.Add(x);

        if (attachments != null)
            foreach (var x in attachments)
                mail.Attachments.Add(new Attachment(x));

        if (bcc != null)
            foreach (var x in bcc)
                mail.Bcc.Add(x);

        if (reply != null)
            foreach (var x in reply)
                mail.ReplyToList.Add(x);

        using var smtp = new SmtpClient(Host, Port.Value)
        {
            Credentials = new NetworkCredential(Username, Password, Domain),
            EnableSsl = EnableSSL
        };
        smtp.Send(mail);
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
}
