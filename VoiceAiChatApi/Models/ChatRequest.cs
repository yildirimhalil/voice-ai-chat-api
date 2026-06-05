using System.ComponentModel.DataAnnotations;

namespace VoiceAiChatApi.Models;

public class ChatRequest
{
    [Required(ErrorMessage = "message alanı zorunludur.")]
    public string Message { get; set; } = string.Empty;
}
