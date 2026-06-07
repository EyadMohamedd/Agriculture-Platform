using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Admin.Services;

public interface IAdminService
{
    // ── User management ───────────────────────────────────────────────────────
    Task<PagedResult<UserManagementDto>> GetAllUsersAsync(PaginationParams pagination);
    Task DeleteUserAsync(string userId);

    // ── System-default validation ranges ─────────────────────────────────────
    Task<List<ValidationRangeDto>> GetValidationRangesAsync();
    Task<ValidationRangeDto> UpdateValidationRangeAsync(string id, ValidationRangeDto dto);

}
