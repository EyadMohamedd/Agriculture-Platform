using AgriculturalMonitorSystem.Src.Features.Sensor.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Services;

public interface ISensorService
{
    Task<PagedResult<SensorReadingDto>> GetReadingsAsync(string? farmId, string userId, PaginationParams pagination, string? sensorType = null, DateTime? from = null, DateTime? to = null);
    Task<LatestReadingDto> GetLatestReadingsByFarmAsync(string farmId, string userId);
    Task<SensorStatisticsDto> GetStatisticsAsync(string farmId, string sensorType, DateTime from, DateTime to, string userId);
    Task SaveReadingAsync(SensorReading reading);
}
