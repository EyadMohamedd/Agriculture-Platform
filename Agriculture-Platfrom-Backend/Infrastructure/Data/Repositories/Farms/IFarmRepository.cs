using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Farms;

public interface IFarmRepository : IBaseRepository<Farm>
{
    Task<List<Farm>> GetByUserIdAsync(string userId);
    Task<PagedResult<Farm>> GetAllPagedAsync(PaginationParams pagination);
    Task<PagedResult<Farm>> GetByUserIdPagedAsync(string userId, PaginationParams pagination);
    Task<bool> BelongsToUserAsync(string farmId, string userId);
}
