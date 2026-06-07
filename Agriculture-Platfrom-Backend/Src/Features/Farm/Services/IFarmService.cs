using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Farm.Models.DTOs;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Services;

public interface IFarmService
{
    Task<FarmResponseDto> CreateFarmAsync(CreateFarmDto dto, string userId);
    Task<PagedResult<FarmResponseDto>> GetFarmsAsync(string userId, string userRole, PaginationParams pagination);
    Task<FarmResponseDto> UpdateFarmAsync(string farmId, UpdateFarmDto dto, string userId, string userRole);
    Task DeleteFarmAsync(string farmId, string userId, string userRole);

    // ── Farm-specific validation range overrides (hierarchical ranges) ────────
    Task<List<FarmValidationRangeDto>> GetFarmValidationRangesAsync(string farmId, string userId, string userRole);
    Task<FarmValidationRangeDto> UpsertFarmValidationRangeAsync(string farmId, FarmValidationRangeDto dto, string userId, string userRole);
    Task DeleteFarmValidationRangeAsync(string farmId, string rangeId, string userId, string userRole);
}
