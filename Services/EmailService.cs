using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly OAuthHelper _oAuthHelper;

    public EmailService(string smtpServer, int smtpPort, string senderEmail, OAuthHelper oAuthHelper)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _senderEmail = senderEmail;
        _oAuthHelper = oAuthHelper;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        string accessToken = await _oAuthHelper.GetAccessTokenAsync();

        using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
        {
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_senderEmail, accessToken);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(recipientEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
