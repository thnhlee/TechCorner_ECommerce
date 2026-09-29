namespace TechCorner_ECommerce.Services {
    public interface IEmailService {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
