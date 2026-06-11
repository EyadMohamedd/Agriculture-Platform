namespace AgriculturalMonitorSystem.Application.Services.Interfaces;

public interface IResourceOwnershipService
{
    Task<bool> IsFarmOwnerAsync(string userId, string farmId);
    Task<bool> IsSensorOwnerAsync(string userId, string sensorId);
    Task<bool> IsAlertOwnerAsync(string userId, string alertId);
}
