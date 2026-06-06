using VoiceAiChatApi.Models;
using VoiceAiChatApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace VoiceAiChatApi.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "message alanı boş olamaz." });
        }

        var reply = await _chatService.ReplyAsync(request.Message.Trim(), cancellationToken);

        return Ok(new ChatResponse { Response = reply });
    }
}
