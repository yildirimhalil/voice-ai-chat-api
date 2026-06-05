namespace VoiceAiChatApi.Services;

public class ChatOptions
{
    public const string SectionName = "Chat";

    public string Provider { get; set; } = "Static";

    public OllamaOptions Ollama { get; set; } = new();
}

public class OllamaOptions
{
    public string Endpoint { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "llama3.2";

    public string SystemPrompt { get; set; } =
        "Sen bir çağrı merkezi asistanısın. Kullanıcılara kısa, kibar ve Türkçe yanıt ver.";
}
