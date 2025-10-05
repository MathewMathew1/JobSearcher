namespace JobSearch.Emails
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
    }
}