using OpenAI_API;
using VoiceToEmail.Core.Interfaces;

public class ContentService : IContentService
{
    private readonly OpenAIAPI _openAI;
    
    public ContentService(IConfiguration configuration)
    {
        _openAI = new OpenAIAPI(configuration["OpenAI:ApiKey"]);
    }
    
    public async Task<string> EnhanceContentAsync(string transcribedText)
    {
        var chat = _openAI.Chat.CreateConversation();
        
        chat.AppendSystemMessage(@"Transform the following message into a professional email. 
Maintain the core message but make it more formal and well-structured.
Add appropriate greeting and closing.");
        
        chat.AppendUserInput(transcribedText);

        var result = await chat.GetResponseFromChatbotAsync();
        return result.Trim();
    }
}