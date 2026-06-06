using Microsoft.Extensions.Options;

namespace VoiceAiChatApi.Services;

public class ChatService : IChatService
{
    private readonly ChatOptions _options;
    private readonly OllamaResponder _ollama;
    private readonly StaticResponder _static;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IOptions<ChatOptions> options,
        OllamaResponder ollama,
        StaticResponder staticResponder,
        ILogger<ChatService> logger)
    {
        _options = options.Value;
        _ollama = ollama;
        _static = staticResponder;
        _logger = logger;
    }

    public async Task<string> ReplyAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!_options.Provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            return _static.Reply(message);
        }

        try
        {
            return await _ollama.GenerateAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama yanıtı alınamadı, sabit cevaba dönülüyor.");
            return _static.Reply(message);
        }
    }
}
