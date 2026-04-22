using AgriculturalMonitorSystem.Src.Features.Admin.Repositories;
using AgriculturalMonitorSystem.Src.Features.Alert.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Alert.Repositories;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AlertEntity = AgriculturalMonitorSystem.Src.Features.Alert.Models.Entities.Alert;

namespace AgriculturalMonitorSystem.Src.Features.Alert.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IAlertRepository alertRepository,
        IAdminRepository adminRepository,
        ILogger<AlertService> logger)
    {
        _alertRepository = alertRepository;
        _adminRepository = adminRepository;
        _logger = logger;
    }

    public async Task<PagedResult<AlertResponseDto>> GetAlertsAsync(
        string userId, string userRole, PaginationParams pagination)
    {
        var result = userRole == RoleConstants.Admin
            ? await _alertRepository.GetAllPagedAsync(pagination)
            : await _alertRepository.GetByUserIdPagedAsync(userId, pagination);

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
        // ── Hierarchical range lookup ─────────────────────────────────────────
        // Priority: farm-specific override → system default (exact) → system default (base type)
        double minNormal, maxNormal, warningLow, warningHigh, criticalLow, criticalHigh;

        var farmRange = await _adminRepository.GetFarmValidationRangeAsync(farmId, sensorType);
        if (farmRange != null)
        {
            minNormal   = farmRange.MinNormal;  maxNormal   = farmRange.MaxNormal;
            warningLow  = farmRange.WarningLow; warningHigh = farmRange.WarningHigh;
            criticalLow = farmRange.CriticalLow; criticalHigh = farmRange.CriticalHigh;
        }
        else
        {
            var sysRange = await _adminRepository.GetValidationRangeByTypeAsync(sensorType)
                        ?? await _adminRepository.GetValidationRangeByTypeAsync(sensorType.Split('_')[0]);

            if (sysRange == null)
            {
                _logger.LogDebug("No validation range found for sensor type '{SensorType}' — skipping alert check", sensorType);
                return;
            }

            minNormal   = sysRange.MinNormal;  maxNormal   = sysRange.MaxNormal;
            warningLow  = sysRange.WarningLow; warningHigh = sysRange.WarningHigh;
            criticalLow = sysRange.CriticalLow; criticalHigh = sysRange.CriticalHigh;
        }

        // ── Anomaly detection ─────────────────────────────────────────────────
        // Critical  : value < critical_low  OR > critical_high
        // High      : value < warning_low   OR > warning_high
        // Medium    : value outside min_normal/max_normal but within warning
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

        if (severity == null || alertType == null) return;

        // ── Deduplication: no duplicate unresolved alerts for same sensor+type within 1 hour ──
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

        var alert = new AlertEntity
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

    private static AlertResponseDto MapToDto(AlertEntity a) => new()
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
