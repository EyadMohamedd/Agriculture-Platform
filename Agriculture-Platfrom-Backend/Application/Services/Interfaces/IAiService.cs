using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Application.Services.Interfaces;

public interface IAiService
{
    Task<ConversationDto> AskAsync(string userId, string userRole, AskQuestionDto dto);
    Task<PagedResult<ConversationDto>> GetConversationsAsync(string userId, PaginationParams pagination);
    Task<ConversationDto> GetConversationByIdAsync(string userId, string conversationId);
}
