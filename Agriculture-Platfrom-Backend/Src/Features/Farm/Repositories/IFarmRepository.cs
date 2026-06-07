using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;
using FarmEntity = AgriculturalMonitorSystem.Src.Features.Farm.Models.Entities.Farm;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Repositories;

public interface IFarmRepository : ISharedRepository<FarmEntity>
{
    Task<List<FarmEntity>> GetByUserIdAsync(string userId);
    Task<PagedResult<FarmEntity>> GetAllPagedAsync(PaginationParams pagination);
    Task<PagedResult<FarmEntity>> GetByUserIdPagedAsync(string userId, PaginationParams pagination);
    Task<bool> BelongsToUserAsync(string farmId, string userId);
}
