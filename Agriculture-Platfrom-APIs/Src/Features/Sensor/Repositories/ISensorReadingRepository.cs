using AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;

public interface ISensorReadingRepository : ISharedRepository<SensorReading>
{
    Task<PagedResult<SensorReading>> GetByFarmIdPagedAsync(string farmId, PaginationParams pagination);
    Task<List<SensorReading>> GetLatestByFarmIdAsync(string farmId, int limit = 10);
    Task<SensorReading?> GetLatestBySensorIdAsync(string sensorId);
    Task<Dictionary<string, double>> GetStatisticsByFarmIdAsync(string farmId, string sensorType, DateTime from, DateTime to);
    Task DeleteBySensorIdAsync(string sensorId);
    Task DeleteByFarmIdAsync(string farmId);
    Task<long> CountBySensorIdAsync(string sensorId);
    Task<List<SensorReading>> GetRecentBySensorIdAsync(string sensorId, int hours = 1);
}
