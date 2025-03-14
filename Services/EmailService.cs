using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly ILogger<EmailService> _logger; // Add Logger

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _smtpServer = configuration["EmailSettings:Server"];
        _smtpPort = int.Parse(configuration["EmailSettings:Port"]);
        _senderEmail = configuration["EmailSettings:SenderEmail"];
        _smtpUsername = configuration["EmailSettings:Username"];
        _smtpPassword = configuration["EmailSettings:Password"];
        _logger = logger; // Assign Logger
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
{
    try
    {
        _logger.LogInformation("📧 Attempting to send email to {RecipientEmail} via {SmtpServer}:{SmtpPort}",
            recipientEmail, _smtpServer, _smtpPort);

        using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
        {
            smtpClient.EnableSsl = true; // Ensure SSL is enabled
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.TargetName = "STARTTLS/smtp.johnshopkins.edu"; // Force TLS authentication

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(recipientEmail);

            _logger.LogInformation("📨 Sending email to {RecipientEmail}...", recipientEmail);
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("✅ Email successfully sent to {RecipientEmail}", recipientEmail);
        }
    }
    catch (SmtpException smtpEx)
    {
        _logger.LogError(smtpEx, "❌ SMTP Exception: {Message}", smtpEx.Message);
        throw new Exception($"SMTP Error: {smtpEx.Message} | StackTrace: {smtpEx.StackTrace}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ General Exception while sending email: {Message}", ex.Message);
        throw new Exception($"General Error: {ex.Message} | StackTrace: {ex.StackTrace}");
    }
}
}

