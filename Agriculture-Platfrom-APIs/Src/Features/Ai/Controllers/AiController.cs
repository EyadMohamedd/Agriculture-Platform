using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Src.Features.Ai.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Ai.Services;
using AgriculturalMonitorSystem.Src.Shared.Attributes;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Ai.Controllers;

[ApiController]
[Route("api/ai")]
[AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
        => _aiService = aiService;

    // POST /api/ai/ask
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskQuestionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FarmId))
            return BadRequest(ApiResponse.FailureResponse("FarmId is required."));

        if (string.IsNullOrWhiteSpace(dto.Question))
            return BadRequest(ApiResponse.FailureResponse("Question is required."));

        var userId   = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        var result   = await _aiService.AskAsync(userId, userRole, dto);
        return Ok(ApiResponse<ConversationDto>.SuccessResponse(result));
    }

    // GET /api/ai/conversations?page=1&pageSize=20
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] PaginationParams pagination)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var result = await _aiService.GetConversationsAsync(userId, pagination);
        return Ok(ApiResponse<PagedResult<ConversationDto>>.SuccessResponse(result));
    }

    // GET /api/ai/conversations/{id}
    [HttpGet("conversations/{id}")]
    public async Task<IActionResult> GetConversation(string id)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var result = await _aiService.GetConversationByIdAsync(userId, id);
        return Ok(ApiResponse<ConversationDto>.SuccessResponse(result));
    }
}
