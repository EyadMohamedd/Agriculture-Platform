using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Sensors;

public interface ISensorReadingRepository : IBaseRepository<SensorReading>
{
    Task<PagedResult<SensorReading>> GetByFarmIdPagedAsync(string farmId, PaginationParams pagination, string? sensorType = null, DateTime? from = null, DateTime? to = null);
    Task<List<SensorReading>> GetLatestByFarmIdAsync(string farmId, int limit = 10);
    Task<SensorReading?> GetLatestBySensorIdAsync(string sensorId);
    Task<Dictionary<string, double>> GetStatisticsByFarmIdAsync(string farmId, string sensorType, DateTime from, DateTime to);
    Task DeleteBySensorIdAsync(string sensorId);
    Task DeleteByFarmIdAsync(string farmId);
    Task<long> CountBySensorIdAsync(string sensorId);
    Task<List<SensorReading>> GetRecentBySensorIdAsync(string sensorId, int hours = 1);
}
