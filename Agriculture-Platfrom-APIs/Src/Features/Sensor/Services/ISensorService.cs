using AgriculturalMonitorSystem.Src.Features.Sensor.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Services;

public interface ISensorService
{
    Task<PagedResult<SensorReadingDto>> GetReadingsAsync(string? farmId, string userId, string userRole, PaginationParams pagination);
    Task<LatestReadingDto> GetLatestReadingsByFarmAsync(string farmId, string userId, string userRole);
    Task<SensorStatisticsDto> GetStatisticsAsync(string farmId, string sensorType, DateTime from, DateTime to, string userId, string userRole);
    Task SaveReadingAsync(SensorReading reading);
}
