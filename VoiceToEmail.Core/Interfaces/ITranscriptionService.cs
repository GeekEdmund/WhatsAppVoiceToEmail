// ITranscriptionService.cs
namespace VoiceToEmail.Core.Interfaces;

public interface ITranscriptionService
{
    Task<string> TranscribeAudioAsync(byte[] audioData);
}

// IContentService.cs
public interface IContentService
{
    Task<string> EnhanceContentAsync(string transcribedText);
}

// IEmailService.cs
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string content);
}