using Microsoft.AspNetCore.Mvc;
using VoiceToEmail.Core.Interfaces;

namespace VoiceToEmail.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ITranscriptionService _transcriptionService;
    private readonly IContentService _contentService;
    private readonly IEmailService _emailService;
    private readonly ILogger<MessageController> _logger;
    
    public MessageController(
        ITranscriptionService transcriptionService,
        IContentService contentService,
        IEmailService emailService,
        ILogger<MessageController> logger)
    {
        _transcriptionService = transcriptionService;
        _contentService = contentService;
        _emailService = emailService;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> SendMessage(IFormFile audioFile, string recipientEmail)
    {
        try
        {
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest("Audio file is required");

            using var memoryStream = new MemoryStream();
            await audioFile.CopyToAsync(memoryStream);
            var audioData = memoryStream.ToArray();

            _logger.LogInformation("Starting transcription");
            var transcribedText = await _transcriptionService.TranscribeAudioAsync(audioData);

            _logger.LogInformation("Enhancing content");
            var enhancedContent = await _contentService.EnhanceContentAsync(transcribedText);

            _logger.LogInformation("Sending email");
            await _emailService.SendEmailAsync(
                recipientEmail,
                "Voice Message Transcription",
                enhancedContent
            );

            var response = new
            {
                TranscribedText = transcribedText,
                EnhancedContent = enhancedContent,
                RecipientEmail = recipientEmail,
                Status = "Completed"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing voice message");
            return StatusCode(500, "An error occurred while processing your message");
        }
    }
}