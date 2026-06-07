using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;
using AlertEntity = AgriculturalMonitorSystem.Src.Features.Alert.Models.Entities.Alert;

namespace AgriculturalMonitorSystem.Src.Features.Alert.Repositories;

public interface IAlertRepository : ISharedRepository<AlertEntity>
{
    Task<PagedResult<AlertEntity>> GetAllPagedAsync(PaginationParams pagination);
    Task<PagedResult<AlertEntity>> GetByUserIdPagedAsync(string userId, PaginationParams pagination, string? farmId = null, string? severity = null);
    Task<AlertEntity?> GetUnresolvedBySensorAndTypeAsync(string sensorId, string alertType, DateTime since);
    Task<List<AlertEntity>> GetActiveByFarmIdAsync(string farmId);
    Task ResolveBySensorTypePrefixAndSeveritiesAsync(string sensorId, string alertTypePrefix, IEnumerable<string> severities);
    Task DeleteByFarmIdAsync(string farmId);
    Task DeleteBySensorIdAsync(string sensorId);
    Task<string?> GetFarmIdByAlertIdAsync(string alertId);
}
