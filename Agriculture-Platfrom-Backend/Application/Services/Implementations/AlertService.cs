using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Admin;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Alerts;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgriculturalMonitorSystem.Application.Services.Implementations;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IValidationRangeRepository _validationRangeRepository;
    private readonly IFarmValidationRangeRepository _farmValidationRangeRepository;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IAlertRepository alertRepository,
        IValidationRangeRepository validationRangeRepository,
        IFarmValidationRangeRepository farmValidationRangeRepository,
        ILogger<AlertService> logger)
    {
        _alertRepository = alertRepository;
        _validationRangeRepository = validationRangeRepository;
        _farmValidationRangeRepository = farmValidationRangeRepository;
        _logger = logger;
    }

    public async Task<PagedResult<AlertResponseDto>> GetAlertsAsync(
        string userId, PaginationParams pagination, string? farmId = null, string? severity = null)
    {
        var result = await _alertRepository.GetByUserIdPagedAsync(userId, pagination, farmId, severity);

        return new PagedResult<AlertResponseDto>
        {
            Items      = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task ProcessReadingForAlertsAsync(
        string sensorId, string farmId, string userId, string sensorType, double value)
    {
        double minNormal, maxNormal, warningLow, warningHigh, criticalLow, criticalHigh;

        var farmRange = await _farmValidationRangeRepository.GetFarmValidationRangeAsync(farmId, sensorType);
        if (farmRange != null)
        {
            minNormal   = farmRange.MinNormal;  maxNormal   = farmRange.MaxNormal;
            warningLow  = farmRange.WarningLow; warningHigh = farmRange.WarningHigh;
            criticalLow = farmRange.CriticalLow; criticalHigh = farmRange.CriticalHigh;
        }
        else
        {
            var sysRange = await _validationRangeRepository.GetValidationRangeByTypeAsync(sensorType)
                        ?? await _validationRangeRepository.GetValidationRangeByTypeAsync(sensorType.Split('_')[0]);

            if (sysRange == null)
            {
                _logger.LogDebug("No validation range found for sensor type '{SensorType}' — skipping alert check", sensorType);
                return;
            }

            minNormal   = sysRange.MinNormal;  maxNormal   = sysRange.MaxNormal;
            warningLow  = sysRange.WarningLow; warningHigh = sysRange.WarningHigh;
            criticalLow = sysRange.CriticalLow; criticalHigh = sysRange.CriticalHigh;
        }

        string? severity = null;
        string? alertType = null;
        double threshold = 0;

        if (value < criticalLow)
        {
            severity  = "Critical"; alertType = $"{sensorType}_critical_low"; threshold = criticalLow;
        }
        else if (value > criticalHigh)
        {
            severity  = "Critical"; alertType = $"{sensorType}_critical_high"; threshold = criticalHigh;
        }
        else if (value < warningLow)
        {
            severity  = "High"; alertType = $"{sensorType}_warning_low"; threshold = warningLow;
        }
        else if (value > warningHigh)
        {
            severity  = "High"; alertType = $"{sensorType}_warning_high"; threshold = warningHigh;
        }
        else if (value < minNormal)
        {
            severity  = "Medium"; alertType = $"{sensorType}_below_normal"; threshold = minNormal;
        }
        else if (value > maxNormal)
        {
            severity  = "Medium"; alertType = $"{sensorType}_above_normal"; threshold = maxNormal;
        }

        string[] allSeverities = ["Critical", "High", "Medium"];
        var toResolve = severity == null
            ? allSeverities
            : allSeverities.Where(s => s != severity).ToArray();
        await _alertRepository.ResolveBySensorTypePrefixAndSeveritiesAsync(sensorId, sensorType, toResolve);

        if (severity == null || alertType == null) return;

        var since = DateTime.UtcNow.AddHours(-1);
        var existing = await _alertRepository.GetUnresolvedBySensorAndTypeAsync(sensorId, alertType, since);

        if (existing != null)
        {
            existing.Timestamp = DateTime.UtcNow;
            await _alertRepository.UpdateAsync(existing.Id, existing);
            return;
        }

        var friendlyType = char.ToUpper(sensorType[0]) + sensorType[1..];
        var message = $"{friendlyType} reached {value:F2} ({severity.ToLower()} threshold: {threshold})";

        var alert = new Alert
        {
            FarmId     = farmId,
            SensorId   = sensorId,
            UserId     = userId,
            Type       = alertType,
            Severity   = severity,
            Message    = message,
            Timestamp  = DateTime.UtcNow,
            IsResolved = false
        };

        await _alertRepository.InsertAsync(alert);
        _logger.LogWarning("Alert created [{Severity}]: {Message}", severity, message);
    }

    private static AlertResponseDto MapToDto(Alert a) => new()
    {
        Id         = a.Id,
        FarmId     = a.FarmId,
        SensorId   = a.SensorId,
        UserId     = a.UserId,
        Type       = a.Type,
        Severity   = a.Severity,
        Message    = a.Message,
        Timestamp  = a.Timestamp,
        IsResolved = a.IsResolved,
        ResolvedAt = a.ResolvedAt
    };
}
