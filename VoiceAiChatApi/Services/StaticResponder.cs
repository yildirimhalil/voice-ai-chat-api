namespace VoiceAiChatApi.Services;

public class StaticResponder
{
    public string Reply(string message)
    {
        var normalized = message.ToLowerInvariant();

        if (Contains(normalized, "merhaba", "selam", "günaydın", "iyi günler"))
        {
            return "Merhaba, size nasıl yardımcı olabilirim?";
        }

        if (Contains(normalized, "teşekkür", "sağ ol", "sagol"))
        {
            return "Rica ederim, başka bir konuda yardımcı olabilir miyim?";
        }

        if (Contains(normalized, "fiyat", "ücret", "tarife"))
        {
            return "Fiyatlandırma bilgisi için sizi ilgili birime aktarabilirim.";
        }

        if (Contains(normalized, "görüşürüz", "hoşça kal", "kapat"))
        {
            return "İyi günler dileriz, görüşmek üzere.";
        }

        return "Sorunuzu aldım, en kısa sürede size yardımcı olacağım.";
    }

    private static bool Contains(string source, params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (source.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
