namespace VoiceToEmail.Core.Models;

public class WhatsAppMessage
{
    public string? MessageSid { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Body { get; set; }
    public int NumMedia { get; set; }
    public Dictionary<string, string> MediaUrls { get; set; } = new();
}

public class ConversationState
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? PendingVoiceNoteUrl { get; set; }
    public bool WaitingForEmail { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Method to check if the state is stale
    public bool IsStale => DateTime.UtcNow.Subtract(LastUpdated).TotalHours > 24;
}