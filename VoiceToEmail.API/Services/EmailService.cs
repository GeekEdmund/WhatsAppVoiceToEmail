using SendGrid;
using SendGrid.Helpers.Mail;
using VoiceToEmail.Core.Interfaces;

public class EmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;
    
    public EmailService(IConfiguration configuration)
    {
        var apiKey = configuration["SendGrid:ApiKey"] ?? 
            throw new ArgumentNullException("SendGrid:ApiKey configuration is missing");
        _client = new SendGridClient(apiKey);
        
        _fromEmail = configuration["SendGrid:FromEmail"] ?? 
            throw new ArgumentNullException("SendGrid:FromEmail configuration is missing");
        _fromName = configuration["SendGrid:FromName"] ?? 
            throw new ArgumentNullException("SendGrid:FromName configuration is missing");
    }
    
    public async Task SendEmailAsync(string to, string subject, string content)
    {
        var from = new EmailAddress(_fromEmail, _fromName);
        var toAddress = new EmailAddress(to);
        
        var msg = MailHelper.CreateSingleEmail(
            from,
            toAddress,
            subject,
            content,
            $"<div style='font-family: Arial, sans-serif;'>{content}</div>"
        );
        
        var response = await _client.SendEmailAsync(msg);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to send email: {response.StatusCode}");
        }
    }
}