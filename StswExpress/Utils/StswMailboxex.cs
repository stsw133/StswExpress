using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Collections.ObjectModel;

namespace StswExpress;

/// <summary>
/// Provides functionality for managing email configurations and methods to import and export them.
/// </summary>
public static class StswMailboxex
{
    /// <summary>
    /// The dictionary that contains all declared email configurations for application.
    /// </summary>
    public static ObservableCollection<StswMailboxModel> Collection { get; set; } = [];

    /// <summary>
    /// Default instance of email configuration (that is currently in use by application). 
    /// </summary>
    public static StswMailboxModel? Current { get; set; }

    /// <summary>
    /// Specifies the location of the file where email configurations are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "mailboxes.stsw");

    /// <summary>
    /// Reads email configurations from a file specified by <see cref="FilePath"/> and saves them in <see cref="Collection"/>.
    /// </summary>
    public static void ImportList()
    {
        Collection.Clear();

        if (!File.Exists(FilePath))
        {
            new FileInfo(FilePath)?.Directory?.Create();
            File.Create(FilePath).Close();
        }

        using var stream = new StreamReader(FilePath);
        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();
            if (line != null)
            {
                var data = line.Split('|');
                Collection.Add(new StswMailboxModel()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Host = StswSecurity.Decrypt(data[1]),
                    Port = StswSecurity.Decrypt(data[2]) is string s1 && !string.IsNullOrEmpty(s1) ? Convert.ToInt32(s1) : 0,
                    Address = StswSecurity.Decrypt(data[3]),
                    Username = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5]),
                    Domain = StswSecurity.Decrypt(data[6]) is string s2 && !string.IsNullOrEmpty(s2) ? s2 : null,
                    EnableSSL = StswSecurity.Decrypt(data[7]) is string s3 && !string.IsNullOrEmpty(s3) && Convert.ToBoolean(s3)
                });
            }
        }
    }

    /// <summary>
    /// Writes email configurations to a file specified by <see cref="FilePath"/>.
    /// </summary>
    public static void ExportList()
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var mc in Collection)
            stream.WriteLine(StswSecurity.Encrypt(mc.Name)
                    + "|" + StswSecurity.Encrypt(mc.Host)
                    + "|" + StswSecurity.Encrypt(mc.Port.ToString() ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.Address)
                    + "|" + StswSecurity.Encrypt(mc.Username)
                    + "|" + StswSecurity.Encrypt(mc.Password)
                    + "|" + StswSecurity.Encrypt(mc.Domain ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.EnableSSL.ToString() ?? string.Empty)
                );
    }
}

/// <summary>
/// Represents an email account and provides a way to send emails using that account.
/// </summary>
public class StswMailboxModel : StswObservableObject
{
    /// <summary>
    /// Gets or sets the name of the email account.
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string _name = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP host for the email account.
    /// </summary>
    public string Host
    {
        get => _host;
        set => SetProperty(ref _host, value);
    }
    private string _host = string.Empty;

    /// <summary>
    /// Gets or sets the port number for the SMTP host.
    /// </summary>
    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }
    private int _port = 0;

    /// <summary>
    /// Gets or sets the email address associated with the email account.
    /// </summary>
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }
    private string _address = string.Empty;

    /// <summary>
    /// Gets or sets the username for the email account.
    /// </summary>
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }
    private string _username = string.Empty;

    /// <summary>
    /// Gets or sets the password for the email account.
    /// </summary>
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    private string _password = string.Empty;

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

        using var smtp = new SmtpClient(Host, Port)
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
    public void Send(IEnumerable<string> to, string subject, string body, IEnumerable<string>? attachments = null) => Send(to, subject, body, attachments, BCC, ReplyTo);

    /// <summary>
    /// Sends an email using the SMTP protocol without attachments.
    /// </summary>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    public void Send(IEnumerable<string> to, string subject, string body) => Send(to, subject, body, null, BCC, ReplyTo);
}
