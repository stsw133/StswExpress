using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace StswExpress
{
	public class Mail
	{
		/// <summary>
		/// SendMail
		/// </summary>
		public static bool SendMail(string from, string[] to, string subject, string body, string[] attachments = null, string[] bbc = null, string[] reply = null)
		{
			try
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

					var smtp = new SmtpClient(Settings.Default.mail_Host, Settings.Default.mail_Port)
					{
						Credentials = new NetworkCredential(Settings.Default.mail_Username, Security.Decrypt(Settings.Default.mail_Password)),
						EnableSsl = true
					};

					smtp.Send(mail);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{MethodBase.GetCurrentMethod().Name}{Environment.NewLine}Błąd wysyłki wiadomości e-mail:{Environment.NewLine}{ex.Message}");
				return false;
			}

			return true;
		}

        /// <summary>
        /// SendMail
        /// </summary>
        public static bool SendMail(string from, string to, string subject, string body, string[] attachments = null, string[] bbc = null, string[] reply = null) =>
            SendMail(from, new string[] { to }, subject, body, attachments, bbc, reply);

    }
}
