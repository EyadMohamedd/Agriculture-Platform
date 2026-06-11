using Microsoft.Extensions.Caching.Memory;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Alerts;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Farms;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Sensors;
using AgriculturalMonitorSystem.Application.Services.Interfaces;

namespace AgriculturalMonitorSystem.Application.Services.Implementations;

/// <summary>
/// Answers ownership questions for the FarmOwnershipMiddleware.
/// Results are cached for 5 minutes per resource to avoid repeated DB hits.
/// </summary>
public class ResourceOwnershipService : IResourceOwnershipService
{
    private readonly IFarmRepository _farmRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public ResourceOwnershipService(
        IFarmRepository farmRepository,
        ISensorRepository sensorRepository,
        IAlertRepository alertRepository,
        IMemoryCache cache)
    {
        _farmRepository = farmRepository;
        _sensorRepository = sensorRepository;
        _alertRepository = alertRepository;
        _cache = cache;
    }

    public async Task<bool> IsFarmOwnerAsync(string userId, string farmId)
    {
        var cacheKey = $"farm_owner_{farmId}";

        if (_cache.TryGetValue(cacheKey, out string? cachedOwnerId))
            return cachedOwnerId == userId;

        var farm = await _farmRepository.GetByIdAsync(farmId);
        if (farm == null) return false;

        _cache.Set(cacheKey, farm.UserId, CacheTtl);
        return farm.UserId == userId;
    }

    public async Task<bool> IsSensorOwnerAsync(string userId, string sensorId)
    {
        var cacheKey = $"sensor_farmowner_{sensorId}";

        if (!_cache.TryGetValue(cacheKey, out string? cachedFarmId))
        {
            cachedFarmId = await _sensorRepository.GetFarmIdBySensorIdAsync(sensorId);
            if (cachedFarmId == null) return false;
            _cache.Set(cacheKey, cachedFarmId, CacheTtl);
        }

        return await IsFarmOwnerAsync(userId, cachedFarmId!);
    }

    public async Task<bool> IsAlertOwnerAsync(string userId, string alertId)
    {
        var cacheKey = $"alert_farmowner_{alertId}";

        if (!_cache.TryGetValue(cacheKey, out string? cachedFarmId))
        {
            cachedFarmId = await _alertRepository.GetFarmIdByAlertIdAsync(alertId);
            if (cachedFarmId == null) return false;
            _cache.Set(cacheKey, cachedFarmId, CacheTtl);
        }

        return await IsFarmOwnerAsync(userId, cachedFarmId!);
    }
}
