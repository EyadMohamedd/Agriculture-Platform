using AgriculturalMonitorSystem.Src.Features.Farm.Repositories;
using AgriculturalMonitorSystem.Src.Features.Sensor.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Exceptions;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Services;

public class SensorService : ISensorService
{
    private readonly ISensorRepository _sensorRepository;
    private readonly ISensorReadingRepository _readingRepository;
    private readonly IFarmRepository _farmRepository;

    public SensorService(
        ISensorRepository sensorRepository,
        ISensorReadingRepository readingRepository,
        IFarmRepository farmRepository)
    {
        _sensorRepository = sensorRepository;
        _readingRepository = readingRepository;
        _farmRepository = farmRepository;
    }

    public async Task<PagedResult<SensorReadingDto>> GetReadingsAsync(
        string? farmId, string userId, string userRole, PaginationParams pagination)
    {
        if (farmId != null)
        {
            await ValidateFarmAccessAsync(farmId, userId, userRole);
            var result = await _readingRepository.GetByFarmIdPagedAsync(farmId, pagination);
            return MapPagedResult(result);
        }

        // No farmId: Admins see all (across all farms), Farmers see own
        if (userRole == RoleConstants.Admin)
        {
            var farms = await _farmRepository.GetAllAsync();
            var allReadings = new List<SensorReadingDto>();
            foreach (var farm in farms)
            {
                var r = await _readingRepository.GetLatestByFarmIdAsync(farm.Id, pagination.PageSize);
                allReadings.AddRange(r.Select(MapToDto));
            }
            return new PagedResult<SensorReadingDto>
            {
                Items = allReadings.Take(pagination.PageSize).ToList(),
                TotalCount = allReadings.Count,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            };
        }
        else
        {
            var userFarms = await _farmRepository.GetByUserIdAsync(userId);
            var allReadings = new List<SensorReadingDto>();
            foreach (var farm in userFarms)
            {
                var r = await _readingRepository.GetByFarmIdPagedAsync(farm.Id, pagination);
                allReadings.AddRange(r.Items.Select(MapToDto));
            }
            return new PagedResult<SensorReadingDto>
            {
                Items = allReadings.Take(pagination.PageSize).ToList(),
                TotalCount = allReadings.Count,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            };
        }
    }

    public async Task<LatestReadingDto> GetLatestReadingsByFarmAsync(string farmId, string userId, string userRole)
    {
        await ValidateFarmAccessAsync(farmId, userId, userRole);

        var sensors = await _sensorRepository.GetByFarmIdAsync(farmId);
        var readings = new Dictionary<string, LatestSensorValue>();

        foreach (var sensor in sensors)
        {
            var latest = await _readingRepository.GetLatestBySensorIdAsync(sensor.Id);
            readings[sensor.SensorType] = new LatestSensorValue
            {
                SensorId = sensor.Id,
                SensorName = sensor.SensorName,
                Value = ExtractValue(latest, sensor.SensorType),
                Timestamp = latest?.Timestamp,
                IsAnomaly = latest?.IsAnomaly ?? false
            };
        }

        return new LatestReadingDto { FarmId = farmId, Readings = readings };
    }

    public async Task<SensorStatisticsDto> GetStatisticsAsync(
        string farmId, string sensorType, DateTime from, DateTime to, string userId, string userRole)
    {
        await ValidateFarmAccessAsync(farmId, userId, userRole);

        var stats = await _readingRepository.GetStatisticsByFarmIdAsync(farmId, sensorType, from, to);

        return new SensorStatisticsDto
        {
            FarmId = farmId,
            SensorType = sensorType,
            From = from,
            To = to,
            Min   = stats.TryGetValue("min",   out var mn)  ? mn  : null,
            Max   = stats.TryGetValue("max",   out var mx)  ? mx  : null,
            Avg   = stats.TryGetValue("avg",   out var av)  ? av  : null,
            Count = stats.TryGetValue("count", out var cnt) ? (int)cnt : 0
        };
    }

    public async Task SaveReadingAsync(SensorReading reading)
        => await _readingRepository.InsertAsync(reading);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task ValidateFarmAccessAsync(string farmId, string userId, string userRole)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId)
            ?? throw new NotFoundException(ErrorMessages.FarmNotFound);
        if (userRole != RoleConstants.Admin && farm.UserId != userId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);
    }

    private static double? ExtractValue(SensorReading? r, string sensorType) => sensorType switch
    {
        "temperature" => r?.Temperature,
        "ph"          => r?.SoilPh,
        "moisture"    => r?.SoilMoisture,
        "npk"         => r?.NpkN,
        "rainfall"    => r?.Rainfall,
        _             => null
    };

    private static SensorReadingDto MapToDto(SensorReading r) => new()
    {
        Id           = r.Id,
        SensorId     = r.SensorId,
        FarmId       = r.FarmId,
        Timestamp    = r.Timestamp,
        Temperature  = r.Temperature,
        SoilPh       = r.SoilPh,
        SoilMoisture = r.SoilMoisture,
        NpkN         = r.NpkN,
        NpkP         = r.NpkP,
        NpkK         = r.NpkK,
        Rainfall     = r.Rainfall,
        IsAnomaly    = r.IsAnomaly,
        AnomalyReason = r.AnomalyReason
    };

    private static PagedResult<SensorReadingDto> MapPagedResult(PagedResult<SensorReading> src) => new()
    {
        Items      = src.Items.Select(MapToDto).ToList(),
        TotalCount = src.TotalCount,
        Page       = src.Page,
        PageSize   = src.PageSize
    };
}
