﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace StswExpress;

/// Model for Mail Configuration
public class MC
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSSL { get; set; } = false;

    /// Load list of encrypted mail configs from file.
    public static List<MC> LoadAllMailConfigs(string path)
    {
        var result = new List<MC>();

        if (!File.Exists(path))
            File.Create(path).Close();

        using var stream = new StreamReader(path);
        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();
            if (line != null)
            {
                var data = line.Split('|');
                result.Add(new MC()
                {
                    Name = Security.Decrypt(data[0]),
                    Host = Security.Decrypt(data[1]),
                    Port = Convert.ToInt32(Security.Decrypt(data[2])),
                    Address = Security.Decrypt(data[3]),
                    Username = Security.Decrypt(data[4]),
                    Password = Security.Decrypt(data[5]),
                    EnableSSL = Convert.ToBoolean(Security.Decrypt(data[6]))
                });
            }
        }

        return result;
    }

    /// Save list of encrypted mail configs to file.
    public static void SaveAllMailConfigs(List<MC> mailConfigs, string path)
    {
        using var stream = new StreamWriter(path);
        foreach (var mc in mailConfigs)
            stream.WriteLine(Security.Encrypt(mc.Name)
                + "|" + Security.Encrypt(mc.Host)
                + "|" + Security.Encrypt(mc.Port.ToString())
                + "|" + Security.Encrypt(mc.Address)
                + "|" + Security.Encrypt(mc.Username)
                + "|" + Security.Encrypt(mc.Password)
                + "|" + Security.Encrypt(mc.EnableSSL.ToString()));
    }

    /// Send mail to recipients.
    public bool Send(IEnumerable<string> to, string subject, string body, IEnumerable<string>? attachments = null, IEnumerable<string>? bbc = null, IEnumerable<string>? reply = null)
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

            if (bbc != null)
                foreach (var x in bbc)
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
