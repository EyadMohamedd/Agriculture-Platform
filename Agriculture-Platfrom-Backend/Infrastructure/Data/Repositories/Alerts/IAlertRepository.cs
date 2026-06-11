using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Alerts;

public interface IAlertRepository : IBaseRepository<Alert>
{
    Task<PagedResult<Alert>> GetAllPagedAsync(PaginationParams pagination);
    Task<PagedResult<Alert>> GetByUserIdPagedAsync(string userId, PaginationParams pagination, string? farmId = null, string? severity = null);
    Task<Alert?> GetUnresolvedBySensorAndTypeAsync(string sensorId, string alertType, DateTime since);
    Task<List<Alert>> GetActiveByFarmIdAsync(string farmId);
    Task ResolveBySensorTypePrefixAndSeveritiesAsync(string sensorId, string alertTypePrefix, IEnumerable<string> severities);
    Task DeleteByFarmIdAsync(string farmId);
    Task DeleteBySensorIdAsync(string sensorId);
    Task<string?> GetFarmIdByAlertIdAsync(string alertId);
}
