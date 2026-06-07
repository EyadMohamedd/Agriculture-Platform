using AgriculturalMonitorSystem.Src.Features.Farm.Repositories;
using AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;
using AgriculturalMonitorSystem.Src.Shared.Interfaces;

namespace AgriculturalMonitorSystem.Src.Shared.Services;

/// <summary>
/// Counts dependent documents before deletion so callers can decide whether
/// to block (user has farms) or cascade (farm has sensors).
/// </summary>
public class ReferenceChecker : IReferenceChecker
{
    private readonly IFarmRepository _farmRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly ISensorReadingRepository _readingRepository;

    public ReferenceChecker(
        IFarmRepository farmRepository,
        ISensorRepository sensorRepository,
        ISensorReadingRepository readingRepository)
    {
        _farmRepository = farmRepository;
        _sensorRepository = sensorRepository;
        _readingRepository = readingRepository;
    }

    public async Task<Dictionary<string, int>> CheckUserReferencesAsync(string userId)
    {
        var farms = await _farmRepository.GetByUserIdAsync(userId);
        return new Dictionary<string, int>
        {
            { "farms", farms.Count }
        };
    }

    public async Task<Dictionary<string, int>> CheckFarmReferencesAsync(string farmId)
    {
        var sensorCount = (int)await _sensorRepository.CountByFarmIdAsync(farmId);
        return new Dictionary<string, int>
        {
            { "sensors", sensorCount }
        };
    }

    public async Task<Dictionary<string, int>> CheckSensorReferencesAsync(string sensorId)
    {
        var readingCount = (int)await _readingRepository.CountBySensorIdAsync(sensorId);
        return new Dictionary<string, int>
        {
            { "readings", readingCount }
        };
    }
}
