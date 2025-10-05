
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;


namespace JobSearch.Emails
{
    public class EmailSettings
{
    public string Provider { get; set; } = "smtp";
    public string From { get; set; } = "";
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public string AwsAccessKeyId { get; set; } = "";
    public string AwsSecretAccessKey { get; set; } = "";
    public string AwsRegion { get; set; } = "us-east-1";
}

public class SmtpEmailService : IEmailService, IDisposable
{
    private readonly EmailSettings _settings;

    public SmtpEmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder();
        if (!string.IsNullOrEmpty(plainTextBody)) builder.TextBody = plainTextBody;
        builder.HtmlBody = htmlBody;
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public void Dispose() { /* nothing to dispose */ }
}
}

