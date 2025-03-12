using OpenAI.Chat;
using VoiceToEmail.Core.Interfaces;

public class ContentService : IContentService
{
    private readonly ChatClient _client;

    public ContentService(IConfiguration configuration)
    {
        string apiKey = configuration["OpenAI:ApiKey"];
        // Initialize the ChatClient with your model (e.g. "03-mini")
        _client = new ChatClient(model: "o3-mini-2025-01-31", apiKey: apiKey);
    }

    public async Task<string> EnhanceContentAsync(string transcribedText)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "Transform the following message into a professional email. " +
                "Maintain the core message but make it more formal and well-structured. " +
                "Add appropriate greeting and closing."
            ),
            new UserChatMessage(transcribedText)
        };

        var response = await _client.CompleteChatAsync(messages);
        return response.Value.Content.Last().Text.Trim();
    }
}
