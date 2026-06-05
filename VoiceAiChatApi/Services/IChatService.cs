namespace VoiceAiChatApi.Services;

public interface IChatService
{
    Task<string> ReplyAsync(string message, CancellationToken cancellationToken = default);
}
