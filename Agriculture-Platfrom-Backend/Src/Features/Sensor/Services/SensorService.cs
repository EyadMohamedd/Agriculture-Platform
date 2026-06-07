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
        string? farmId, string userId, PaginationParams pagination,
        string? sensorType = null, DateTime? from = null, DateTime? to = null)
    {
        if (farmId != null)
        {
            await ValidateFarmAccessAsync(farmId, userId);
            var result = await _readingRepository.GetByFarmIdPagedAsync(farmId, pagination, sensorType, from, to);
            return MapPagedResult(result);
        }

        var userFarms = await _farmRepository.GetByUserIdAsync(userId);
        var allReadings = new List<SensorReadingDto>();
        foreach (var farm in userFarms)
        {
            var r = await _readingRepository.GetByFarmIdPagedAsync(farm.Id, pagination, sensorType, from, to);
            allReadings.AddRange(r.Items.Select(MapToDto));
        }
        return new PagedResult<SensorReadingDto>
        {
            Items      = allReadings.Take(pagination.PageSize).ToList(),
            TotalCount = allReadings.Count,
            Page       = pagination.Page,
            PageSize   = pagination.PageSize
        };
    }

    public async Task<LatestReadingDto> GetLatestReadingsByFarmAsync(string farmId, string userId)
    {
        await ValidateFarmAccessAsync(farmId, userId);

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
        string farmId, string sensorType, DateTime from, DateTime to, string userId)
    {
        await ValidateFarmAccessAsync(farmId, userId);

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

    private async Task ValidateFarmAccessAsync(string farmId, string userId)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId)
            ?? throw new NotFoundException(ErrorMessages.FarmNotFound);
        if (farm.UserId != userId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);
    }

    private static Dictionary<string, double?> ExtractValue(SensorReading? r, string sensorType) => sensorType switch
    {
        "temperature" => new() { ["temperature"] = r?.Temperature },
        "ph"          => new() { ["ph"]          = r?.SoilPh },
        "moisture"    => new() { ["moisture"]    = r?.SoilMoisture },
        "npk"         => new() { ["n"] = r?.NpkN, ["p"] = r?.NpkP, ["k"] = r?.NpkK },
        "rainfall"    => new() { ["rainfall"]    = r?.Rainfall },
        _             => new()
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
