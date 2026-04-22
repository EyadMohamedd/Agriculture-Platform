using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Admin.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Admin.Repositories;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Auth.Repositories;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Exceptions;
using AgriculturalMonitorSystem.Src.Shared.Interfaces;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Admin.Services;

public class AdminService : IAdminService
{
    private readonly IAuthRepository _authRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IDeleteService _deleteService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IAuthRepository authRepository,
        IAdminRepository adminRepository,
        IDeleteService deleteService,
        ILogger<AdminService> logger)
    {
        _authRepository = authRepository;
        _adminRepository = adminRepository;
        _deleteService = deleteService;
        _logger = logger;
    }

    // ── User management ───────────────────────────────────────────────────────

    public async Task<PagedResult<UserManagementDto>> GetAllUsersAsync(PaginationParams pagination)
    {
        var result = await _authRepository.GetUsersPagedAsync(pagination);
        return new PagedResult<UserManagementDto>
        {
            Items      = result.Items.Select(MapUserToDto).ToList(),
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

    public async Task<ValidationRangeDto> CreateValidationRangeAsync(ValidationRangeDto dto)
    {
        if (await _adminRepository.ValidationRangeExistsForTypeAsync(dto.SensorType))
            throw new ConflictException(ErrorMessages.ValidationRangeAlreadyExists);

        var range = new ValidationRange
        {
            SensorType  = dto.SensorType,
            MinNormal   = dto.MinNormal,
            MaxNormal   = dto.MaxNormal,
            WarningLow  = dto.WarningLow,
            WarningHigh = dto.WarningHigh,
            CriticalLow = dto.CriticalLow,
            CriticalHigh = dto.CriticalHigh
        };

        await _adminRepository.InsertAsync(range);
        _logger.LogInformation("Validation range created for sensor type '{SensorType}'", dto.SensorType);
        return MapRangeToDto(range);
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

    private static UserManagementDto MapUserToDto(User u) => new()
    {
        Id           = u.Id,
        Name         = u.Name,
        Email        = u.Email,
        Phone        = u.Phone,
        FarmLocation = u.FarmLocation
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
