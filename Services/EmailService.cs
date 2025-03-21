using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
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
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _smtpServer = configuration["EmailSettings:Server"];
        _smtpPort = int.Parse(configuration["EmailSettings:Port"]);
        _senderEmail = configuration["EmailSettings:SenderEmail"];
        _smtpUsername = configuration["EmailSettings:Username"];
        _smtpPassword = configuration["EmailSettings:Password"];
        _logger = logger;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body, byte[] attachmentBytes = null, string attachmentFilename = "Attachment.pdf")
    {
        try
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true;

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_senderEmail);
                    mailMessage.To.Add(recipientEmail);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    if (attachmentBytes != null)
                    {
                        var attachmentStream = new MemoryStream(attachmentBytes);
                        var attachment = new Attachment(attachmentStream, attachmentFilename, MediaTypeNames.Application.Pdf);
                        mailMessage.Attachments.Add(attachment);
                    }

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"✅ Email sent successfully to {recipientEmail}.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error sending email to {recipientEmail}: {ex.Message}");
            throw;
        }
    }
}
