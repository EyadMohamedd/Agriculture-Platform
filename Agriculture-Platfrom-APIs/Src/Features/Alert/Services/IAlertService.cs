using AgriculturalMonitorSystem.Src.Features.Alert.Models.DTOs;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Alert.Services;

public interface IAlertService
{
    Task<PagedResult<AlertResponseDto>> GetAlertsAsync(string userId, string userRole, PaginationParams pagination);

    /// <summary>
    /// Evaluate a sensor reading against validation thresholds and create/update alerts.
    /// Called by SensorSimulationService after each reading is saved.
    /// </summary>
    Task ProcessReadingForAlertsAsync(
        string sensorId, string farmId, string userId, string sensorType, double value);
}
