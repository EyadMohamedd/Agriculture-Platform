using AgriculturalMonitorSystem.Src.Features.Admin.Repositories;
using AgriculturalMonitorSystem.Src.Features.Ai.Repositories;
using AgriculturalMonitorSystem.Src.Features.Alert.Repositories;
using AgriculturalMonitorSystem.Src.Features.Auth.Repositories;
using AgriculturalMonitorSystem.Src.Features.Farm.Repositories;
using AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Exceptions;
using AgriculturalMonitorSystem.Src.Shared.Interfaces;

namespace AgriculturalMonitorSystem.Src.Shared.Services;

/// <summary>
/// Hard-delete with reference integrity checks and cascade rules:
/// <list type="bullet">
///   <item>User    → BLOCK if user has farms (unless force=true)</item>
///   <item>Farm    → CASCADE to sensors, readings, alerts, farm validation ranges</item>
///   <item>Sensor  → CASCADE to readings and alerts for that sensor</item>
/// </list>
/// </summary>
public class DeleteService : IDeleteService
{
    private readonly IAuthRepository _authRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly ISensorReadingRepository _readingRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IAiRepository _aiRepository;
    private readonly IReferenceChecker _referenceChecker;
    private readonly ILogger<DeleteService> _logger;

    public DeleteService(
        IAuthRepository authRepository,
        IFarmRepository farmRepository,
        ISensorRepository sensorRepository,
        ISensorReadingRepository readingRepository,
        IAlertRepository alertRepository,
        IAdminRepository adminRepository,
        IAiRepository aiRepository,
        IReferenceChecker referenceChecker,
        ILogger<DeleteService> logger)
    {
        _authRepository   = authRepository;
        _farmRepository   = farmRepository;
        _sensorRepository = sensorRepository;
        _readingRepository = readingRepository;
        _alertRepository  = alertRepository;
        _adminRepository  = adminRepository;
        _aiRepository     = aiRepository;
        _referenceChecker = referenceChecker;
        _logger           = logger;
    }

    /// <summary>
    /// Deletes a user. When force=true, all farms owned by the user are cascade-deleted first
    /// (sensors, readings, alerts, validation ranges). When force=false, blocks if any farms exist.
    /// </summary>
    public async Task<bool> DeleteUserAsync(string userId, bool force = false)
    {
        if (force)
        {
            var userFarms = await _farmRepository.GetByUserIdAsync(userId);
            foreach (var farm in userFarms)
                await DeleteFarmAsync(farm.Id);
        }
        else
        {
            var refs = await _referenceChecker.CheckUserReferencesAsync(userId);
            if (refs.TryGetValue("farms", out var farmCount) && farmCount > 0)
                throw new BadRequestException(ErrorMessages.UserHasFarms);
        }

        await _authRepository.DeletePasswordResetTokensByUserIdAsync(userId);
        var deleted = await _authRepository.DeleteAsync(userId);
        _logger.LogInformation("User {UserId} deleted (force={Force})", userId, force);
        return deleted;
    }

    /// <summary>
    /// Deletes a farm and cascades to:
    /// sensors → their readings → their alerts → farm-level alerts → farm validation ranges → farm
    /// </summary>
    public async Task<bool> DeleteFarmAsync(string farmId, bool force = false)
    {
        var sensors = await _sensorRepository.GetByFarmIdAsync(farmId);

        foreach (var sensor in sensors)
        {
            // Cascade each sensor's readings and sensor-specific alerts
            await _readingRepository.DeleteBySensorIdAsync(sensor.Id);
            await _alertRepository.DeleteBySensorIdAsync(sensor.Id);
        }

        // Delete all sensors for this farm
        await _sensorRepository.DeleteByFarmIdAsync(farmId);

        // Delete farm-level readings/alerts that weren't caught per-sensor
        await _readingRepository.DeleteByFarmIdAsync(farmId);
        await _alertRepository.DeleteByFarmIdAsync(farmId);

        // Delete custom validation range overrides for this farm
        await _adminRepository.DeleteFarmValidationRangesByFarmIdAsync(farmId);

        // Delete AI conversations for this farm
        await _aiRepository.DeleteByFarmIdAsync(farmId);

        var deleted = await _farmRepository.DeleteAsync(farmId);
        _logger.LogInformation("Farm {FarmId} deleted with full cascade ({SensorCount} sensors removed)",
            farmId, sensors.Count);
        return deleted;
    }

    /// <summary>
    /// Deletes a sensor and cascades to its readings and alerts.
    /// </summary>
    public async Task<bool> DeleteSensorAsync(string sensorId, bool force = false)
    {
        await _readingRepository.DeleteBySensorIdAsync(sensorId);
        await _alertRepository.DeleteBySensorIdAsync(sensorId);

        var deleted = await _sensorRepository.DeleteAsync(sensorId);
        _logger.LogInformation("Sensor {SensorId} deleted with cascade", sensorId);
        return deleted;
    }
}
