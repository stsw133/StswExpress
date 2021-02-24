using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace StswExpress.Globals
{
	public class Mail
	{
		public static string Host { get; set; }
		public static int Port { get; set; }
		public static string Email { get; set; }
		public static string Password { get; set; }

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

					var smtp = new SmtpClient(Host, Port)
					{
						Credentials = new NetworkCredential(Email, Global.Decrypt(Password)),
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
	}
}
