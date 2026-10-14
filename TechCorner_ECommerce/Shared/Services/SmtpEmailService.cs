using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace TechCorner_ECommerce.Services {
    public class SmtpEmailService : IEmailService {
        private readonly EmailSettings settings;
        private readonly ILogger<SmtpEmailService> logger;

        public SmtpEmailService(IOptions<EmailSettings> options, ILogger<SmtpEmailService> logger) {
            settings = options.Value;
            this.logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody) {
            if (!settings.Enabled) {
                logger.LogInformation("Email sending is disabled. Subject: {Subject}, To: {ToEmail}", subject, toEmail);
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail) ||
                string.IsNullOrWhiteSpace(toEmail)) {
                logger.LogWarning("Email settings are incomplete. Subject: {Subject}, To: {ToEmail}", subject, toEmail);
                return;
            }

            try {
                using var message = new MailMessage {
                    From = new MailAddress(settings.FromEmail, settings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                using var client = new SmtpClient(settings.Host, settings.Port) {
                    EnableSsl = settings.EnableSsl
                };

                if (!string.IsNullOrWhiteSpace(settings.UserName)) {
                    client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
                }

                await client.SendMailAsync(message);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Could not send email. Subject: {Subject}, To: {ToEmail}", subject, toEmail);
            }
        }
    }
}
