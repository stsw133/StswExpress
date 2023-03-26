using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace StswExpress;

/// Model for Mail Config
public class StswMailConfig
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSSL { get; set; } = false;

    /// FilePath
    public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mailconfigs.stsw");

    /// <summary>
    /// Loads list of encrypted mail configs from file.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>List of mail configs</returns>
    public static List<StswMailConfig> LoadAllMailConfigs()
    {
        var result = new List<StswMailConfig>();

        if (!File.Exists(FilePath))
            File.Create(FilePath).Close();

        using var stream = new StreamReader(FilePath);
        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();
            if (line != null)
            {
                var data = line.Split('|');
                result.Add(new StswMailConfig()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Host = StswSecurity.Decrypt(data[1]),
                    Port = Convert.ToInt32(StswSecurity.Decrypt(data[2])),
                    Address = StswSecurity.Decrypt(data[3]),
                    Username = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5]),
                    EnableSSL = Convert.ToBoolean(StswSecurity.Decrypt(data[6]))
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Saves list of encrypted mail configs to file.
    /// </summary>
    /// <param name="mailConfigs">Mail configs</param>
    /// <param name="path"></param>
    public static void SaveAllMailConfigs(List<StswMailConfig> mailConfigs)
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var mc in mailConfigs)
            stream.WriteLine(StswSecurity.Encrypt(mc.Name)
                + "|" + StswSecurity.Encrypt(mc.Host)
                + "|" + StswSecurity.Encrypt(mc.Port.ToString())
                + "|" + StswSecurity.Encrypt(mc.Address)
                + "|" + StswSecurity.Encrypt(mc.Username)
                + "|" + StswSecurity.Encrypt(mc.Password)
                + "|" + StswSecurity.Encrypt(mc.EnableSSL.ToString()));
    }

    /// <summary>
    /// Sends mail to recipients.
    /// </summary>
    /// <param name="to">Recipients</param>
    /// <param name="subject">Subject of message</param>
    /// <param name="body">Body of message</param>
    /// <param name="attachments">Attachments of message</param>
    /// <param name="bcc">BCC</param>
    /// <param name="reply">Reply to</param>
    /// <returns></returns>
    public bool Send(IEnumerable<string> to, string subject, string body, IEnumerable<string>? attachments = null, IEnumerable<string>? bcc = null, IEnumerable<string>? reply = null)
    {
        using (var mail = new MailMessage())
        {
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

        return true;
    }
}
