using AgriculturalMonitorSystem.Src.Features.Ai.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Ai.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Ai.Repositories;
using AgriculturalMonitorSystem.Src.Shared.Exceptions;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Services;

namespace AgriculturalMonitorSystem.Src.Features.Ai.Services;

public class AiService : IAiService
{
    private readonly IAiRepository _aiRepository;
    private readonly EnvironmentContextBuilder _contextBuilder;
    private readonly AiHttpClient _aiHttpClient;
    private readonly ILogger<AiService> _logger;

    public AiService(
        IAiRepository aiRepository,
        EnvironmentContextBuilder contextBuilder,
        AiHttpClient aiHttpClient,
        ILogger<AiService> logger)
    {
        _aiRepository   = aiRepository;
        _contextBuilder = contextBuilder;
        _aiHttpClient   = aiHttpClient;
        _logger         = logger;
    }

    public async Task<ConversationDto> AskAsync(string userId, string userRole, AskQuestionDto dto)
    {
        var context = await _contextBuilder.BuildAsync(dto.FarmId, userId, userRole);

        var request = new AiRequestDto
        {
            Question   = dto.Question,
            SensorData = context
        };

        var answer = await _aiHttpClient.AskAsync(request);

        var conversation = new AiConversation
        {
            UserId          = userId,
            FarmId          = dto.FarmId,
            Question        = dto.Question,
            Answer          = answer,
            ContextSnapshot = context,
            Timestamp       = DateTime.UtcNow
        };

        await _aiRepository.InsertAsync(conversation);
        _logger.LogInformation("AI conversation saved for user {UserId}, farm {FarmId}", userId, dto.FarmId);

        return MapToDto(conversation);
    }

    public async Task<PagedResult<ConversationDto>> GetConversationsAsync(string userId, PaginationParams pagination)
    {
        var result = await _aiRepository.GetByUserIdPagedAsync(userId, pagination);
        return new PagedResult<ConversationDto>
        {
            Items      = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<ConversationDto> GetConversationByIdAsync(string userId, string conversationId)
    {
        var conversation = await _aiRepository.GetByIdAsync(conversationId)
            ?? throw new NotFoundException("Conversation not found.");

        if (conversation.UserId != userId)
            throw new ForbiddenException("You do not have access to this conversation.");

        return MapToDto(conversation);
    }

    private static ConversationDto MapToDto(AiConversation c) => new()
    {
        Id              = c.Id,
        FarmId          = c.FarmId,
        Question        = c.Question,
        Answer          = c.Answer,
        ContextSnapshot = c.ContextSnapshot,
        Timestamp       = c.Timestamp
    };
}
