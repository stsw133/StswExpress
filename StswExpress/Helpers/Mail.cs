using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace StswExpress
{
    public class Mail
    {
        /// Send mail to recipients.
        public static bool Send(List<string> to, string subject, string body, List<string> attachments = null, List<string> bbc = null, List<string> reply = null)
        {
            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(Fn.AppMailConfig.Address);
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

                var smtp = new SmtpClient(Fn.AppMailConfig.Host, Fn.AppMailConfig.Port)
                {
                    Credentials = new NetworkCredential(Fn.AppMailConfig.Username, Fn.AppMailConfig.Password),
                    EnableSsl = Fn.AppMailConfig.EnableSSL
                };

                smtp.Send(mail);
            }

            return true;
        }
    }
}
