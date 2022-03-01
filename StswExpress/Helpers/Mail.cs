using System.Net;
using System.Net.Mail;

namespace StswExpress
{
    public class Mail
    {
        private static string host = string.Empty;
        public static string Host { set { host = value; } }

        private static int port = 0;
        public static int Port { set { port = value; } }

        private static string username = string.Empty;
        private static string Username { set { username = value; } }

        private static string password = string.Empty;
        private static string Password { set { password = value; } }

        /// Send mail to recipients.
        public static bool Send(string from, string[] to, string subject, string body, string[] attachments = null, string[] bbc = null, string[] reply = null, bool ssl = true)
        {
            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(from);
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

                var smtp = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = ssl
                };

                smtp.Send(mail);
            }

            return true;
        }
    }
}
