namespace BaseConLogin.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendNewsletterAsync(List<string> emails, string subject, string htmlMessage);
    }
}
