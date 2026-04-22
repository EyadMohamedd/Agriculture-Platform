namespace AgriculturalMonitorSystem.Src.Shared.Interfaces;

public interface IDeleteService
{
    /// <summary>Deletes a user. Blocks if user has farms (use force=true only for cascade scenarios).</summary>
    Task<bool> DeleteUserAsync(string userId, bool force = false);

    /// <summary>Deletes a farm and cascades to sensors, readings, and alerts.</summary>
    Task<bool> DeleteFarmAsync(string farmId, bool force = false);

    /// <summary>Deletes a sensor and cascades to its readings and alerts.</summary>
    Task<bool> DeleteSensorAsync(string sensorId, bool force = false);
}
