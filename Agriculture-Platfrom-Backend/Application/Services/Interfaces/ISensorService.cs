using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Application.Services.Interfaces;

public interface ISensorService
{
    Task<PagedResult<SensorReadingDto>> GetReadingsAsync(string? farmId, string userId, PaginationParams pagination, string? sensorType = null, DateTime? from = null, DateTime? to = null);
    Task<LatestReadingDto> GetLatestReadingsByFarmAsync(string farmId, string userId);
    Task<SensorStatisticsDto> GetStatisticsAsync(string farmId, string sensorType, DateTime from, DateTime to, string userId);
    Task SaveReadingAsync(SensorReading reading);
}
