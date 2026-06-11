using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Ai;

public interface IAiConversationRepository : IBaseRepository<AiConversation>
{
    Task<PagedResult<AiConversation>> GetByUserIdPagedAsync(string userId, PaginationParams pagination);
    Task DeleteByFarmIdAsync(string farmId);
    Task DeleteByUserIdAsync(string userId);
}
