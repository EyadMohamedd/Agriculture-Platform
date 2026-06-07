using AgriculturalMonitorSystem.Src.Features.Ai.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;

namespace AgriculturalMonitorSystem.Src.Features.Ai.Repositories;

public interface IAiRepository : ISharedRepository<AiConversation>
{
    Task<PagedResult<AiConversation>> GetByUserIdPagedAsync(string userId, PaginationParams pagination);
    Task DeleteByFarmIdAsync(string farmId);
    Task DeleteByUserIdAsync(string userId);
}
