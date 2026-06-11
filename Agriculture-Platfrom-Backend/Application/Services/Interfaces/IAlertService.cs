using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Application.Services.Interfaces;

public interface IAlertService
{
    Task<PagedResult<AlertResponseDto>> GetAlertsAsync(string userId, PaginationParams pagination, string? farmId = null, string? severity = null);

    /// <summary>
    /// Evaluate a sensor reading against validation thresholds and create/update alerts.
    /// Called by SensorSimulationService after each reading is saved.
    /// </summary>
    Task ProcessReadingForAlertsAsync(
        string sensorId, string farmId, string userId, string sensorType, double value);
}
