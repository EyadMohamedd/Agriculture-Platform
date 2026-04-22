using AgriculturalMonitorSystem.Src.Shared.Repositories;
using SensorEntity = AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities.Sensor;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;

public interface ISensorRepository : ISharedRepository<SensorEntity>
{
    Task<List<SensorEntity>> GetByFarmIdAsync(string farmId);
    Task<List<SensorEntity>> GetActiveSensorsAsync();
    Task<long> CountByFarmIdAsync(string farmId);
    Task DeleteByFarmIdAsync(string farmId);
    Task<string?> GetFarmIdBySensorIdAsync(string sensorId);
}
