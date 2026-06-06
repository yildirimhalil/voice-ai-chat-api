using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace VoiceAiChatApi.Services;

public class OllamaResponder
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaResponder(HttpClient httpClient, IOptions<ChatOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value.Ollama;
    }

    public async Task<string> GenerateAsync(string message, CancellationToken cancellationToken)
    {
        var payload = new OllamaRequest
        {
            Model = _options.Model,
            System = _options.SystemPrompt,
            Prompt = message,
            Stream = false
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var url = $"{_options.Endpoint.TrimEnd('/')}/api/generate";
        var httpResponse = await _httpClient.PostAsync(url, content, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var parsed = JsonSerializer.Deserialize<OllamaResponse>(body);

        var text = parsed?.Response?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            throw new InvalidOperationException("Model boş yanıt döndürdü.");
        }

        return text;
    }

    private class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("system")]
        public string System { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }
}
