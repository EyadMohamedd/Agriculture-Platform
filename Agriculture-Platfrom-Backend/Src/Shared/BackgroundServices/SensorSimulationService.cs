using AgriculturalMonitorSystem.Config;
using AgriculturalMonitorSystem.Src.Features.Alert.Services;
using AgriculturalMonitorSystem.Src.Features.Farm.Repositories;
using AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;
using AgriculturalMonitorSystem.Src.Features.Sensor.Services;

namespace AgriculturalMonitorSystem.Src.Shared.BackgroundServices;

public class SensorSimulationService : BackgroundService
{
    private readonly SimulationSettings  _settings;
    private readonly CsvReadingProvider  _csvProvider;
    private readonly IServiceProvider    _services;
    private readonly ILogger<SensorSimulationService> _logger;

    public SensorSimulationService(
        SimulationSettings settings,
        CsvReadingProvider csvProvider,
        IServiceProvider services,
        ILogger<SensorSimulationService> logger)
    {
        _settings    = settings;
        _csvProvider = csvProvider;
        _services    = services;
        _logger      = logger;
    }

    // ── BackgroundService entry point ─────────────────────────────────────────

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SensorSimulationService started (interval={IntervalSeconds}s, source=CSV).",
            _settings.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), stoppingToken)
                .ConfigureAwait(false);

            await RunCycleAsync(stoppingToken);
        }
    }

    // ── Simulation cycle ──────────────────────────────────────────────────────

    private async Task RunCycleAsync(CancellationToken ct)
    {
        using var scope   = _services.CreateScope();
        var sensorRepo    = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
        var farmRepo      = scope.ServiceProvider.GetRequiredService<IFarmRepository>();
        var sensorSvc     = scope.ServiceProvider.GetRequiredService<ISensorService>();
        var alertSvc      = scope.ServiceProvider.GetRequiredService<IAlertService>();

        var activeSensors = await sensorRepo.GetActiveSensorsAsync();
        if (activeSensors.Count == 0)
        {
            _logger.LogDebug("Simulation cycle: no active sensors found — skipping");
            return;
        }

        // Group sensors by farm so each farm consumes one CSV row per cycle
        var sensorsByFarm = activeSensors.GroupBy(s => s.FarmId);

        _logger.LogInformation("Simulation cycle: {FarmCount} farm(s), {SensorCount} sensor(s)",
            sensorsByFarm.Count(), activeSensors.Count);

        foreach (var farmGroup in sensorsByFarm)
        {
            if (ct.IsCancellationRequested) break;

            var farm = await farmRepo.GetByIdAsync(farmGroup.Key);
            if (farm == null) continue;

            var row = _csvProvider.GetNext();

            foreach (var sensor in farmGroup)
            {
                try
                {
                    var reading = BuildReading(sensor, row);
                    await sensorSvc.SaveReadingAsync(reading);
                    await EvaluateAlertsAsync(alertSvc, sensor, farm.UserId, reading);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating reading for sensor {SensorId}", sensor.Id);
                }
            }
        }
    }

    // ── Reading builder ───────────────────────────────────────────────────────

    private static SensorReading BuildReading(Sensor sensor, CsvSensorRow row)
    {
        var reading = new SensorReading
        {
            SensorId  = sensor.Id,
            FarmId    = sensor.FarmId,
            Timestamp = DateTime.UtcNow,
        };

        switch (sensor.SensorType)
        {
            case "temperature": reading.Temperature  = row.Temperature;  break;
            case "ph":          reading.SoilPh       = row.SoilPh;       break;
            case "moisture":    reading.SoilMoisture = row.SoilMoisture; break;
            case "rainfall":    reading.Rainfall     = row.Rainfall;     break;
            case "npk":
                reading.NpkN = row.NpkN;
                reading.NpkP = row.NpkP;
                reading.NpkK = row.NpkK;
                break;
        }

        return reading;
    }

    // ── Alert evaluation ──────────────────────────────────────────────────────

    private static async Task EvaluateAlertsAsync(
        IAlertService alertSvc, Sensor sensor, string userId, SensorReading reading)
    {
        switch (sensor.SensorType)
        {
            case "temperature" when reading.Temperature.HasValue:
                await alertSvc.ProcessReadingForAlertsAsync(
                    sensor.Id, sensor.FarmId, userId, "temperature", reading.Temperature.Value);
                break;

            case "ph" when reading.SoilPh.HasValue:
                await alertSvc.ProcessReadingForAlertsAsync(
                    sensor.Id, sensor.FarmId, userId, "ph", reading.SoilPh.Value);
                break;

            case "moisture" when reading.SoilMoisture.HasValue:
                await alertSvc.ProcessReadingForAlertsAsync(
                    sensor.Id, sensor.FarmId, userId, "moisture", reading.SoilMoisture.Value);
                break;

            case "npk":
                if (reading.NpkN.HasValue)
                    await alertSvc.ProcessReadingForAlertsAsync(
                        sensor.Id, sensor.FarmId, userId, "npk_n", reading.NpkN.Value);
                if (reading.NpkP.HasValue)
                    await alertSvc.ProcessReadingForAlertsAsync(
                        sensor.Id, sensor.FarmId, userId, "npk_p", reading.NpkP.Value);
                if (reading.NpkK.HasValue)
                    await alertSvc.ProcessReadingForAlertsAsync(
                        sensor.Id, sensor.FarmId, userId, "npk_k", reading.NpkK.Value);
                break;

            case "rainfall" when reading.Rainfall.HasValue:
                await alertSvc.ProcessReadingForAlertsAsync(
                    sensor.Id, sensor.FarmId, userId, "rainfall", reading.Rainfall.Value);
                break;
        }
    }
}
