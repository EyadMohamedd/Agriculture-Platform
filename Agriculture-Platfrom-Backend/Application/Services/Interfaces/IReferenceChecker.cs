namespace AgriculturalMonitorSystem.Application.Services.Interfaces;

public interface IReferenceChecker
{
    Task<Dictionary<string, int>> CheckUserReferencesAsync(string userId);
    Task<Dictionary<string, int>> CheckFarmReferencesAsync(string farmId);
    Task<Dictionary<string, int>> CheckSensorReferencesAsync(string sensorId);
}
