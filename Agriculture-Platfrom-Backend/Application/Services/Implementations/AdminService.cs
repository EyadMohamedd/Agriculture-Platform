using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Admin;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Auth;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Farms;
using AgriculturalMonitorSystem.Application.Constants;
using AgriculturalMonitorSystem.Application.Exceptions;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgriculturalMonitorSystem.Application.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidationRangeRepository _validationRangeRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IDeleteService _deleteService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepository,
        IValidationRangeRepository validationRangeRepository,
        IFarmRepository farmRepository,
        IDeleteService deleteService,
        ILogger<AdminService> logger)
    {
        _userRepository  = userRepository;
        _validationRangeRepository = validationRangeRepository;
        _farmRepository  = farmRepository;
        _deleteService   = deleteService;
        _logger          = logger;
    }

    public async Task<PagedResult<UserManagementDto>> GetAllUsersAsync(PaginationParams pagination)
    {
        var result = await _userRepository.GetUsersPagedAsync(pagination);

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
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        if (user.Role == RoleConstants.Admin && await _userRepository.IsLastAdminAsync(userId))
            throw new BadRequestException(ErrorMessages.LastAdminProtected);

        await _deleteService.DeleteUserAsync(userId, force: true);
        _logger.LogInformation("Admin deleted user {UserId} (cascade)", userId);
    }

    public async Task<List<ValidationRangeDto>> GetValidationRangesAsync()
    {
        var ranges = await _validationRangeRepository.GetAllValidationRangesAsync();
        return ranges.Select(MapRangeToDto).ToList();
    }

    public async Task<ValidationRangeDto> UpdateValidationRangeAsync(string id, ValidationRangeDto dto)
    {
        var range = await _validationRangeRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(ErrorMessages.ValidationRangeNotFound);

        range.SensorType  = dto.SensorType;
        range.MinNormal   = dto.MinNormal;
        range.MaxNormal   = dto.MaxNormal;
        range.WarningLow  = dto.WarningLow;
        range.WarningHigh = dto.WarningHigh;
        range.CriticalLow = dto.CriticalLow;
        range.CriticalHigh = dto.CriticalHigh;

        await _validationRangeRepository.UpdateAsync(id, range);
        _logger.LogInformation("Validation range {Id} updated", id);
        return MapRangeToDto(range);
    }

    private static UserManagementDto MapUserToDto(User u, IEnumerable<Farm> farms) => new()
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
