using AgriculturalMonitorSystem.Src.Features.Ai.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Alert.Repositories;
using AgriculturalMonitorSystem.Src.Features.Farm.Repositories;
using AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;
using AgriculturalMonitorSystem.Src.Shared.Exceptions;
using AgriculturalMonitorSystem.Src.Shared.Constants;

namespace AgriculturalMonitorSystem.Src.Shared.Services;

public class EnvironmentContextBuilder
{
    private readonly IFarmRepository _farmRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly ISensorReadingRepository _readingRepository;
    private readonly IAlertRepository _alertRepository;

    public EnvironmentContextBuilder(
        IFarmRepository farmRepository,
        ISensorRepository sensorRepository,
        ISensorReadingRepository readingRepository,
        IAlertRepository alertRepository)
    {
        _farmRepository   = farmRepository;
        _sensorRepository = sensorRepository;
        _readingRepository = readingRepository;
        _alertRepository  = alertRepository;
    }

    public async Task<AiSensorDataDto> BuildAsync(string farmId, string userId, string userRole)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId)
            ?? throw new NotFoundException(ErrorMessages.FarmNotFound);

        if (userRole != RoleConstants.Admin && farm.UserId != userId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);

        var sensors  = await _sensorRepository.GetByFarmIdAsync(farmId);
        var context  = new AiSensorDataDto
        {
            FarmLocation = farm.Location.FormattedAddress ?? string.Empty,
            CropType     = farm.CropType ?? string.Empty,
        };

        foreach (var sensor in sensors)
        {
            var latest = await _readingRepository.GetLatestBySensorIdAsync(sensor.Id);
            if (latest == null) continue;

            switch (sensor.SensorType)
            {
                case "temperature": context.Temperature  = latest.Temperature;  break;
                case "ph":          context.SoilPh       = latest.SoilPh;       break;
                case "moisture":    context.SoilMoisture = latest.SoilMoisture; break;
                case "rainfall":    context.RainfallMm   = latest.Rainfall;     break;
                case "npk":
                    context.Nitrogen    = latest.NpkN;
                    context.Phosphorus  = latest.NpkP;
                    context.Potassium   = latest.NpkK;
                    break;
            }
        }

        var activeAlerts = await _alertRepository.GetActiveByFarmIdAsync(farmId);

        // Send only High/Critical alerts to the RAG, one per sensor (highest severity wins)
        var severityOrder = new Dictionary<string, int> { ["Critical"] = 0, ["High"] = 1 };
        context.Alerts = activeAlerts
            .Where(a => a.Severity == "Critical" || a.Severity == "High")
            .GroupBy(a => MapAlertTypeToSensor(a.Type))
            .Select(g => g.OrderBy(a => severityOrder.GetValueOrDefault(a.Severity, 99)).First())
            .Select(a => new AlertContextDto
            {
                Severity = a.Severity.ToLower(),
                Sensor   = MapAlertTypeToSensor(a.Type),
                Message  = a.Message
            }).ToList();

        return context;
    }

    private static string MapAlertTypeToSensor(string alertType)
    {
        if (alertType.StartsWith("npk_n")) return "nitrogen";
        if (alertType.StartsWith("npk_p")) return "phosphorus";
        if (alertType.StartsWith("npk_k")) return "potassium";

        return alertType.Split('_')[0] switch
        {
            "temperature" => "temperature",
            "ph"          => "soil_ph",
            "moisture"    => "soil_moisture",
            "rainfall"    => "rainfall_mm",
            _             => alertType
        };
    }
}
