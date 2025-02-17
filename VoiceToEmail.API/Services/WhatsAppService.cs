using System.Net.Http.Headers;
using Twilio;
using VoiceToEmail.Core.Interfaces;
using VoiceToEmail.Core.Models;

namespace VoiceToEmail.API.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly IConfiguration _configuration;
    private readonly ITranscriptionService _transcriptionService;
    private readonly IContentService _contentService;
    private readonly IEmailService _emailService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WhatsAppService> _logger;
    private static readonly Dictionary<string, ConversationState> _conversationStates = new();
    private static readonly object _stateLock = new();

    public WhatsAppService(
        IConfiguration configuration,
        ITranscriptionService transcriptionService,
        IContentService contentService,
        IEmailService emailService,
        HttpClient httpClient,
        ILogger<WhatsAppService> logger)
    {
        _configuration = configuration;
        _transcriptionService = transcriptionService;
        _contentService = contentService;
        _emailService = emailService;
        _httpClient = httpClient;
        _logger = logger;

        // Initialize Twilio client
        var accountSid = configuration["Twilio:AccountSid"] ?? 
            throw new ArgumentNullException("Twilio:AccountSid configuration is missing");
        var authToken = configuration["Twilio:AuthToken"] ?? 
            throw new ArgumentNullException("Twilio:AuthToken configuration is missing");

        // Set up HTTP client authentication for Twilio media downloads
        var authString = Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", authString);

        TwilioClient.Init(accountSid, authToken);
        
        _logger.LogInformation("WhatsAppService initialized successfully");
    }

    public async Task<string> HandleIncomingMessageAsync(WhatsAppMessage message)
    {
        try
        {
            _logger.LogInformation("Processing incoming message from {From}", message.From);

            ConversationState state;
            lock (_stateLock)
            {
                if (!_conversationStates.TryGetValue(message.From!, out state!))
                {
                    state = new ConversationState { PhoneNumber = message.From! };
                    _conversationStates[message.From!] = state;
                    _logger.LogInformation("Created new conversation state for {From}", message.From);
                }
            }

            // If waiting for email address
            if (state.WaitingForEmail && !string.IsNullOrEmpty(message.Body))
            {
                return await HandleEmailProvided(message.Body, state);
            }

            // If it's a voice note
            if (message.NumMedia > 0 && message.MediaUrls.Any())
            {
                return await HandleVoiceNote(message.MediaUrls.First().Value, state);
            }

            // Default response
            return "Please send a voice note to convert it to email, or type an email address if requested.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing incoming message from {From}", message.From);
            throw;
        }
    }

    private async Task<string> HandleVoiceNote(string mediaUrl, ConversationState state)
    {
        try
        {
            _logger.LogInformation("Downloading voice note from {MediaUrl}", mediaUrl);

            // Download the voice note
            byte[] voiceNote;
            try
            {
                voiceNote = await _httpClient.GetByteArrayAsync(mediaUrl);
                _logger.LogInformation("Successfully downloaded voice note ({Bytes} bytes)", voiceNote.Length);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to download media from Twilio. URL: {MediaUrl}, Status: {Status}", 
                    mediaUrl, ex.StatusCode);
                throw;
            }

            // Transcribe the voice note
            var transcription = await _transcriptionService.TranscribeAudioAsync(voiceNote);
            _logger.LogInformation("Successfully transcribed voice note");

            // Extract email address if present
            var emailAddress = ExtractEmailAddress(transcription);

            if (emailAddress != null)
            {
                // Generate and send email
                var enhancedContent = await _contentService.EnhanceContentAsync(transcription);
                await _emailService.SendEmailAsync(emailAddress, "New Message Delivered via WhatsApp Voice-to-Text", enhancedContent);
                _logger.LogInformation("Email sent successfully to {EmailAddress}", emailAddress);
                return "Your voice note has been converted and sent as an email! ✉️";
            }
            else
            {
                // Store voice note URL and wait for email
                state.PendingVoiceNoteUrl = mediaUrl;
                state.WaitingForEmail = true;
                _logger.LogInformation("Waiting for email address from user");
                return "I couldn't find an email address in your message. Please reply with the email address where you'd like to send this message.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing voice note");
            throw;
        }
    }

    private async Task<string> HandleEmailProvided(string emailText, ConversationState state)
    {
        try
        {
            var emailAddress = ExtractEmailAddress(emailText);
            if (emailAddress == null)
            {
                _logger.LogWarning("Invalid email address provided: {EmailText}", emailText);
                return "That doesn't look like a valid email address. Please try again.";
            }

            if (state.PendingVoiceNoteUrl == null)
            {
                _logger.LogWarning("No pending voice note found for {PhoneNumber}", state.PhoneNumber);
                return "Sorry, I couldn't find your voice note. Please send it again.";
            }

            _logger.LogInformation("Processing pending voice note for {EmailAddress}", emailAddress);

            // Download and process the pending voice note
            var voiceNote = await _httpClient.GetByteArrayAsync(state.PendingVoiceNoteUrl);
            var transcription = await _transcriptionService.TranscribeAudioAsync(voiceNote);
            var enhancedContent = await _contentService.EnhanceContentAsync(transcription);
            
            // Send the email
            await _emailService.SendEmailAsync(emailAddress, "New Message Delivered via WhatsApp Voice-to-Text", enhancedContent);

            // Reset state
            state.PendingVoiceNoteUrl = null;
            state.WaitingForEmail = false;

            _logger.LogInformation("Successfully processed voice note and sent email to {EmailAddress}", emailAddress);
            return "Your voice note has been converted and sent as an email! ✉️";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling email provision");
            throw;
        }
    }

    private string? ExtractEmailAddress(string text)
    {
        // Simple regex for email extraction
        var match = System.Text.RegularExpressions.Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        return match.Success ? match.Value : null;
    }
}