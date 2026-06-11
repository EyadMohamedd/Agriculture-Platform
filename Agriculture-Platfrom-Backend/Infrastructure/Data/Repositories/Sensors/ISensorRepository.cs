using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Sensors;

public interface ISensorRepository : IBaseRepository<Sensor>
{
    Task<List<Sensor>> GetByFarmIdAsync(string farmId);
    Task<List<Sensor>> GetActiveSensorsAsync();
    Task<long> CountByFarmIdAsync(string farmId);
    Task DeleteByFarmIdAsync(string farmId);
    Task<string?> GetFarmIdBySensorIdAsync(string sensorId);
}
