
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.mail.Helpers
{
	public class SendMail
	{
		private readonly AppSettings _appSettings;
		private readonly IWebLogger _logger;

		public SendMail(AppSettings appSettings, IWebLogger logger)
		{
			_appSettings = appSettings;
			_logger = logger;
		}
		
		public async Task SendAsync(string toEmail, string toName, string subject, string plainBodyText)
		{
			if ( string.IsNullOrWhiteSpace(_appSettings.MailSmtpServer) )
			{
				_logger.LogInformation("send mail skipped due missing server");
				return;
			}
			
			var message = new MimeMessage ();
			message.From.Add (new MailboxAddress (_appSettings.Name, _appSettings.MailFromEmail));
			if ( string.IsNullOrWhiteSpace(toName) ) toName = toEmail;
			message.To.Add (new MailboxAddress (toName, toEmail));
			message.Subject = subject;

			message.Body = new TextPart ("plain") {
				Text = plainBodyText
			};

			using (var client = new SmtpClient ()) {
				await client.ConnectAsync (_appSettings.MailSmtpServer, _appSettings.MailSmtpPort, _appSettings.MailSmtpUseSsl);

				// Note: only needed if the SMTP server requires authentication
				if ( !string.IsNullOrWhiteSpace(_appSettings.MailSmtpUserName) && !string.IsNullOrWhiteSpace(_appSettings.MailSmtpPassword))
				{
					await client.AuthenticateAsync (_appSettings.MailSmtpUserName, _appSettings.MailSmtpPassword);
				}

				await client.SendAsync(message);
				await client.DisconnectAsync (true);
			}
		}

	}
}
