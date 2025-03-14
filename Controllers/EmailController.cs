using Microsoft.AspNetCore.Mvc;
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
            await _emailService.SendEmailAsync(
                recipientEmail: request.RecipientEmail,
                subject: request.Subject ?? "No Subject",
                body: request.Body ?? "No Body"
            );

            return Ok(new { message = "✅ Email sent successfully!" });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { message = "❌ Failed to send email.", error = ex.Message });
        }
    }
}

// Helper class to map JSON request payload
public class EmailRequest
{
    public string RecipientEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
