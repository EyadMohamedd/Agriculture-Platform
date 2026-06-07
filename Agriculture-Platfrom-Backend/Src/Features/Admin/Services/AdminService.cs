using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Admin.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Admin.Repositories;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Auth.Repositories;
using AgriculturalMonitorSystem.Src.Features.Farm.Repositories;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Exceptions;
using AgriculturalMonitorSystem.Src.Shared.Interfaces;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Admin.Services;

public class AdminService : IAdminService
{
    private readonly IAuthRepository _authRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IDeleteService _deleteService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IAuthRepository authRepository,
        IAdminRepository adminRepository,
        IFarmRepository farmRepository,
        IDeleteService deleteService,
        ILogger<AdminService> logger)
    {
        _authRepository  = authRepository;
        _adminRepository = adminRepository;
        _farmRepository  = farmRepository;
        _deleteService   = deleteService;
        _logger          = logger;
    }

    // ── User management ───────────────────────────────────────────────────────

    public async Task<PagedResult<UserManagementDto>> GetAllUsersAsync(PaginationParams pagination)
    {
        var result = await _authRepository.GetUsersPagedAsync(pagination);

        var dtos = new List<UserManagementDto>();
        foreach (var user in result.Items)
        {
            var farms = await _farmRepository.GetByUserIdAsync(user.Id);
            dtos.Add(MapUserToDto(user, farms));
        }

        return new PagedResult<UserManagementDto>
        {
            Items      = dtos,
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _authRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        if (user.Role == RoleConstants.Admin && await _authRepository.IsLastAdminAsync(userId))
            throw new BadRequestException(ErrorMessages.LastAdminProtected);

        await _deleteService.DeleteUserAsync(userId, force: true);
        _logger.LogInformation("Admin deleted user {UserId} (cascade)", userId);
    }

    // ── System-default validation ranges ─────────────────────────────────────

    public async Task<List<ValidationRangeDto>> GetValidationRangesAsync()
    {
        var ranges = await _adminRepository.GetAllValidationRangesAsync();
        return ranges.Select(MapRangeToDto).ToList();
    }

    public async Task<ValidationRangeDto> UpdateValidationRangeAsync(string id, ValidationRangeDto dto)
    {
        var range = await _adminRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(ErrorMessages.ValidationRangeNotFound);

        range.SensorType  = dto.SensorType;
        range.MinNormal   = dto.MinNormal;
        range.MaxNormal   = dto.MaxNormal;
        range.WarningLow  = dto.WarningLow;
        range.WarningHigh = dto.WarningHigh;
        range.CriticalLow = dto.CriticalLow;
        range.CriticalHigh = dto.CriticalHigh;

        await _adminRepository.UpdateAsync(id, range);
        _logger.LogInformation("Validation range {Id} updated", id);
        return MapRangeToDto(range);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static UserManagementDto MapUserToDto(User u, IEnumerable<Features.Farm.Models.Entities.Farm> farms) => new()
    {
        Id        = u.Id,
        Name      = u.Name,
        Email     = u.Email,
        Phone     = u.Phone,
        Role      = u.Role,
        CreatedAt = u.CreatedAt,
        FarmCount = farms.Count(),
        Farms     = farms.Select(f => new FarmSummaryDto
        {
            Id       = f.Id,
            Name     = f.Name,
            Location = f.Location.FormattedAddress,
            CropType = f.CropType
        }).ToList()
    };

    private static ValidationRangeDto MapRangeToDto(ValidationRange r) => new()
    {
        Id           = r.Id,
        SensorType   = r.SensorType,
        MinNormal    = r.MinNormal,
        MaxNormal    = r.MaxNormal,
        WarningLow   = r.WarningLow,
        WarningHigh  = r.WarningHigh,
        CriticalLow  = r.CriticalLow,
        CriticalHigh = r.CriticalHigh
    };
}
