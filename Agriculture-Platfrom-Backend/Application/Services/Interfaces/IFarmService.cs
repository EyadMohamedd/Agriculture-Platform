using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Application.Services.Interfaces;

public interface IFarmService
{
    Task<FarmResponseDto> CreateFarmAsync(CreateFarmDto dto, string userId);
    Task<PagedResult<FarmResponseDto>> GetFarmsAsync(string userId, string userRole, PaginationParams pagination);
    Task<FarmResponseDto> UpdateFarmAsync(string farmId, UpdateFarmDto dto, string userId, string userRole);
    Task DeleteFarmAsync(string farmId, string userId, string userRole);

    Task<List<FarmValidationRangeDto>> GetFarmValidationRangesAsync(string farmId, string userId, string userRole);
    Task<FarmValidationRangeDto> UpsertFarmValidationRangeAsync(string farmId, FarmValidationRangeDto dto, string userId, string userRole);
    Task DeleteFarmValidationRangeAsync(string farmId, string rangeId, string userId, string userRole);
}
