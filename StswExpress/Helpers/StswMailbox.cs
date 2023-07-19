using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace StswExpress;

/// <summary>
/// Provides functionality for managing email configurations and methods to import and export them.
/// </summary>
public class StswMailbox
{
    /// <summary>
    /// The dictionary that contains all declared email configurations for application.
    /// </summary>
    public static StswDictionary<string, StswMailboxModel> AllMailboxes { get; set; } = new();

    /// <summary>
    /// Default instance of email configuration (that is currently in use by application). 
    /// </summary>
    public static StswMailboxModel? CurrentMailbox { get; set; }

    /// <summary>
    /// Specifies the location of the file where email configurations are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "mailboxes.stsw");

    /// <summary>
    /// Reads email configurations from a file specified by <see cref="FilePath"/> and saves them in <see cref="AllMailboxes"/>.
    /// </summary>
    public static void ImportMailboxes()
    {
        AllMailboxes.Clear();

        if (!File.Exists(FilePath))
        {
            (new FileInfo(FilePath))?.Directory?.Create();
            File.Create(FilePath).Close();
        }

        using var stream = new StreamReader(FilePath);
        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();
            if (line != null)
            {
                var data = line.Split('|');
                AllMailboxes.Add(StswSecurity.Decrypt(data[0]), new StswMailboxModel()
                {
                    Host = StswSecurity.Decrypt(data[1]),
                    Port = Convert.ToInt32(StswSecurity.Decrypt(data[2])),
                    Address = StswSecurity.Decrypt(data[3]),
                    Username = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5]),
                    EnableSSL = Convert.ToBoolean(StswSecurity.Decrypt(data[6]))
                });
            }
        }
    }

    /// <summary>
    /// Writes email configurations to a file specified by <see cref="FilePath"/>.
    /// </summary>
    public static void ExportMailboxes()
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var mc in AllMailboxes)
            stream.WriteLine(StswSecurity.Encrypt(mc.Key)
                    + "|" + StswSecurity.Encrypt(mc.Value.Host)
                    + "|" + StswSecurity.Encrypt(mc.Value.Port.ToString())
                    + "|" + StswSecurity.Encrypt(mc.Value.Address)
                    + "|" + StswSecurity.Encrypt(mc.Value.Username)
                    + "|" + StswSecurity.Encrypt(mc.Value.Password)
                    + "|" + StswSecurity.Encrypt(mc.Value.EnableSSL.ToString())
                );
    }
}

/// <summary>
/// Represents an email account and provides a way to send emails using that account.
/// </summary>
public class StswMailboxModel
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSSL { get; set; } = false;

    /// <summary>
    /// Sends an email using the SMTP protocol.
    /// </summary>
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

        var smtp = new SmtpClient(Host, Port)
        {
            Credentials = new NetworkCredential(Username, Password),
            EnableSsl = EnableSSL
        };

        smtp.Send(mail);
    }
}
