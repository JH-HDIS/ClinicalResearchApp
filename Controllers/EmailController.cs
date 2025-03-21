using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

[Route("api/email")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        if (string.IsNullOrEmpty(request.RecipientEmail))
        {
            return BadRequest(new { message = "Recipient email is required." });
        }

        try
        {
            byte[] attachmentBytes = null;

            // Decode Base64 PDF if provided
            if (!string.IsNullOrEmpty(request.AttachmentBase64))
            {
                try
                {
                    attachmentBytes = Convert.FromBase64String(request.AttachmentBase64);
                }
                catch (FormatException)
                {
                    return BadRequest(new { message = "Invalid Base64 PDF format." });
                }
            }

            // Send email with or without attachment
            await _emailService.SendEmailAsync(
                recipientEmail: request.RecipientEmail,
                subject: request.Subject ?? "No Subject",
                body: request.Body ?? "No Body",
                attachmentBytes: attachmentBytes,
                attachmentFilename: request.AttachmentFilename ?? "Document.pdf"
            );

            return Ok(new { message = "✅ Email sent successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "❌ Failed to send email.", error = ex.Message });
        }
    }
}

// ✅ Updated Helper Class to Accept PDF Attachments
public class EmailRequest
{
    public string RecipientEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string AttachmentBase64 { get; set; } // Base64-encoded PDF
    public string AttachmentFilename { get; set; } // Optional filename for the PDF
}
