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
    public static ObservableCollection<StswMailboxModel> List { get; set; } = new();

    /// <summary>
    /// Default instance of email configuration (that is currently in use by application). 
    /// </summary>
    public static StswMailboxModel? Current { get; set; }

    /// <summary>
    /// Specifies the location of the file where email configurations are stored.
    /// </summary>
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "mailboxes.stsw");

    /// <summary>
    /// Reads email configurations from a file specified by <see cref="FilePath"/> and saves them in <see cref="List"/>.
    /// </summary>
    public static void ImportList()
    {
        List.Clear();

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
                List.Add(new StswMailboxModel()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Host = StswSecurity.Decrypt(data[1]),
                    Port = StswSecurity.Decrypt(data[2]) is string s1 && !string.IsNullOrEmpty(s1) ? Convert.ToInt32(s1) : 0,
                    Address = StswSecurity.Decrypt(data[3]),
                    Username = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5]),
                    EnableSSL = StswSecurity.Decrypt(data[6]) is string s2 && !string.IsNullOrEmpty(s2) ? Convert.ToBoolean(s2) : false
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
        foreach (var mc in List)
            stream.WriteLine(StswSecurity.Encrypt(mc.Name)
                    + "|" + StswSecurity.Encrypt(mc.Host)
                    + "|" + StswSecurity.Encrypt(mc.Port.ToString() ?? string.Empty)
                    + "|" + StswSecurity.Encrypt(mc.Address)
                    + "|" + StswSecurity.Encrypt(mc.Username)
                    + "|" + StswSecurity.Encrypt(mc.Password)
                    + "|" + StswSecurity.Encrypt(mc.EnableSSL.ToString() ?? string.Empty)
                );
    }
}

/// <summary>
/// Represents an email account and provides a way to send emails using that account.
/// </summary>
public class StswMailboxModel : StswObservableObject
{
    /// Name
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }
    private string name = string.Empty;

    /// Host
    public string Host
    {
        get => host;
        set => SetProperty(ref host, value);
    }
    private string host = string.Empty;

    /// Port
    public int Port
    {
        get => port;
        set => SetProperty(ref port, value);
    }
    private int port = 0;

    /// Address
    public string Address
    {
        get => address;
        set => SetProperty(ref address, value);
    }
    private string address = string.Empty;

    /// Username
    public string Username
    {
        get => username;
        set => SetProperty(ref username, value);
    }
    private string username = string.Empty;

    /// Password
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }
    private string password = string.Empty;

    /// EnableSSL
    public bool EnableSSL
    {
        get => enableSSL;
        set => SetProperty(ref enableSSL, value);
    }
    private bool enableSSL = false;

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
