using Microsoft.AspNetCore.Mvc;
using VoiceToEmail.Core.Models;
using VoiceToEmail.Core.Interfaces;

namespace VoiceToEmail.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(
        IWhatsAppService whatsAppService,
        ILogger<WhatsAppController> logger)
    {
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    // Test endpoint to verify routing
    [HttpGet]
    public IActionResult Test()
    {
        _logger.LogInformation("Test endpoint hit at: {time}", DateTime.UtcNow);
        return Ok("WhatsApp endpoint is working!");
    }

    // Main webhook endpoint for Twilio
    [HttpPost]
    public async Task<IActionResult> Webhook([FromForm] Dictionary<string, string> form)
    {
        try
        {
            _logger.LogInformation("Webhook received at: {time}", DateTime.UtcNow);
            
            // Log all incoming form data
            foreach (var item in form)
            {
                _logger.LogInformation("Form data - {Key}: {Value}", item.Key, item.Value);
            }

            // Create WhatsApp message from form data
            var message = new WhatsAppMessage
            {
                MessageSid = form.GetValueOrDefault("MessageSid"),
                From = form.GetValueOrDefault("From"),
                To = form.GetValueOrDefault("To"),
                Body = form.GetValueOrDefault("Body"),
                NumMedia = int.Parse(form.GetValueOrDefault("NumMedia", "0"))
            };

            // Process media if present
            for (int i = 0; i < message.NumMedia; i++)
            {
                var mediaUrl = form.GetValueOrDefault($"MediaUrl{i}");
                var mediaContentType = form.GetValueOrDefault($"MediaContentType{i}");
                if (!string.IsNullOrEmpty(mediaUrl))
                {
                    message.MediaUrls[mediaContentType] = mediaUrl;
                    _logger.LogInformation("Media found - URL: {MediaUrl}, Type: {MediaType}", 
                        mediaUrl, mediaContentType);
                }
            }

            // Process message and get response
            var response = await _whatsAppService.HandleIncomingMessageAsync(message);
            _logger.LogInformation("Response generated: {Response}", response);

            // Create and return TwiML response
            var twimlResponse = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Response>
    <Message>{response}</Message>
</Response>";

            return Content(twimlResponse, "application/xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook: {ErrorMessage}", ex.Message);
            
            // Return a basic TwiML response even in case of error
            var errorResponse = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Response>
    <Message>Sorry, there was an error processing your message. Please try again.</Message>
</Response>";

            return Content(errorResponse, "application/xml");
        }
    }
}