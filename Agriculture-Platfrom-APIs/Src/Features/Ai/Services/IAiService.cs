using AgriculturalMonitorSystem.Src.Features.Ai.Models.DTOs;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Ai.Services;

public interface IAiService
{
    Task<ConversationDto> AskAsync(string userId, string userRole, AskQuestionDto dto);
    Task<PagedResult<ConversationDto>> GetConversationsAsync(string userId, PaginationParams pagination);
    Task<ConversationDto> GetConversationByIdAsync(string userId, string conversationId);
}
