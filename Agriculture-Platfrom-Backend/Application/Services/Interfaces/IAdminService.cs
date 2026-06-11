using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Application.Services.Interfaces;

public interface IAdminService
{
    Task<PagedResult<UserManagementDto>> GetAllUsersAsync(PaginationParams pagination);
    Task DeleteUserAsync(string userId);
    Task<List<ValidationRangeDto>> GetValidationRangesAsync();
    Task<ValidationRangeDto> UpdateValidationRangeAsync(string id, ValidationRangeDto dto);
}
