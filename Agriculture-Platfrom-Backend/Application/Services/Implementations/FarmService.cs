using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Admin;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Auth;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Farms;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Sensors;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Application.Constants;
using AgriculturalMonitorSystem.Application.Exceptions;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgriculturalMonitorSystem.Application.Services.Implementations;

public class FarmService : IFarmService
{
    private readonly IFarmRepository _farmRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly IFarmValidationRangeRepository _farmValidationRangeRepository;
    private readonly IDeleteService _deleteService;
    private readonly ILogger<FarmService> _logger;

    private static readonly string[] SensorTypes = ["temperature", "ph", "moisture", "npk", "rainfall"];

    public FarmService(
        IFarmRepository farmRepository,
        IUserRepository userRepository,
        ISensorRepository sensorRepository,
        IFarmValidationRangeRepository farmValidationRangeRepository,
        IDeleteService deleteService,
        ILogger<FarmService> logger)
    {
        _farmRepository = farmRepository;
        _userRepository = userRepository;
        _sensorRepository = sensorRepository;
        _farmValidationRangeRepository = farmValidationRangeRepository;
        _deleteService = deleteService;
        _logger = logger;
    }

    public async Task<FarmResponseDto> CreateFarmAsync(CreateFarmDto dto, string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        var farm = new Farm
        {
            UserId = userId,
            Name = dto.Name.Trim(),
            Location = dto.Location,
            CropType = string.IsNullOrWhiteSpace(dto.CropType) ? null : dto.CropType.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _farmRepository.InsertAsync(farm);

        var sensors = SensorTypes.Select(type => new Sensor
        {
            FarmId = farm.Id,
            SensorName = $"{farm.Name} {char.ToUpper(type[0]) + type[1..]} Sensor",
            SensorType = type,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        });

        await _sensorRepository.InsertManyAsync(sensors);
        _logger.LogInformation("Farm '{FarmName}' created (id={FarmId}) with 5 sensors for user {UserId}",
            farm.Name, farm.Id, userId);

        return await MapToResponseDtoAsync(farm);
    }

    public async Task<PagedResult<FarmResponseDto>> GetFarmsAsync(string userId, string userRole, PaginationParams pagination)
    {
        PagedResult<Farm> result;

        if (userRole == RoleConstants.Admin)
            result = await _farmRepository.GetAllPagedAsync(pagination);
        else
            result = await _farmRepository.GetByUserIdPagedAsync(userId, pagination);

        var dtos = await Task.WhenAll(result.Items.Select(MapToResponseDtoAsync));

        return new PagedResult<FarmResponseDto>
        {
            Items = [.. dtos],
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<FarmResponseDto> UpdateFarmAsync(string farmId, UpdateFarmDto dto, string userId, string userRole)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId)
            ?? throw new NotFoundException(ErrorMessages.FarmNotFound);

        if (userRole != RoleConstants.Admin && farm.UserId != userId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);

        if (dto.Name != null) farm.Name = dto.Name.Trim();
        if (dto.Location != null) farm.Location = dto.Location;
        if (dto.CropType != null) farm.CropType = string.IsNullOrWhiteSpace(dto.CropType) ? null : dto.CropType.Trim();
        farm.UpdatedAt = DateTime.UtcNow;

        await _farmRepository.UpdateAsync(farmId, farm);
        return await MapToResponseDtoAsync(farm);
    }

    public async Task DeleteFarmAsync(string farmId, string userId, string userRole)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId)
            ?? throw new NotFoundException(ErrorMessages.FarmNotFound);

        if (userRole != RoleConstants.Admin && farm.UserId != userId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);

        await _deleteService.DeleteFarmAsync(farmId);
        _logger.LogInformation("Farm {FarmId} deleted (cascade) by user {UserId}", farmId, userId);
    }

    public async Task<List<FarmValidationRangeDto>> GetFarmValidationRangesAsync(
        string farmId, string userId, string userRole)
    {
        await EnsureFarmAccessAsync(farmId, userId, userRole);
        var ranges = await _farmValidationRangeRepository.GetFarmValidationRangesAsync(farmId);
        return ranges.Select(MapRangeToDto).ToList();
    }

    public async Task<FarmValidationRangeDto> UpsertFarmValidationRangeAsync(
        string farmId, FarmValidationRangeDto dto, string userId, string userRole)
    {
        await EnsureFarmAccessAsync(farmId, userId, userRole);

        var existing = await _farmValidationRangeRepository.GetFarmValidationRangeAsync(farmId, dto.SensorType);
        var range = existing ?? new FarmValidationRange { FarmId = farmId, SensorType = dto.SensorType, CreatedAt = DateTime.UtcNow };

        range.MinNormal  = dto.MinNormal;
        range.MaxNormal  = dto.MaxNormal;
        range.WarningLow = dto.WarningLow;
        range.WarningHigh = dto.WarningHigh;
        range.CriticalLow = dto.CriticalLow;
        range.CriticalHigh = dto.CriticalHigh;
        range.UpdatedAt = DateTime.UtcNow;

        await _farmValidationRangeRepository.UpsertFarmValidationRangeAsync(range);

        var saved = await _farmValidationRangeRepository.GetFarmValidationRangeAsync(farmId, dto.SensorType);
        return MapRangeToDto(saved!);
    }

    public async Task DeleteFarmValidationRangeAsync(
        string farmId, string rangeId, string userId, string userRole)
    {
        await EnsureFarmAccessAsync(farmId, userId, userRole);

        var range = await _farmValidationRangeRepository.GetByIdAsync(rangeId)
            ?? throw new NotFoundException("Farm validation range not found.");

        if (range.FarmId != farmId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);

        await _farmValidationRangeRepository.DeleteAsync(rangeId);
    }

    private async Task EnsureFarmAccessAsync(string farmId, string userId, string userRole)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId)
            ?? throw new NotFoundException(ErrorMessages.FarmNotFound);
        if (userRole != RoleConstants.Admin && farm.UserId != userId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);
    }

    private async Task<FarmResponseDto> MapToResponseDtoAsync(Farm farm)
    {
        var sensorCount = (int)await _sensorRepository.CountByFarmIdAsync(farm.Id);
        return new FarmResponseDto
        {
            Id = farm.Id,
            UserId = farm.UserId,
            Name = farm.Name,
            Location = farm.Location,
            CropType = farm.CropType,
            SensorCount = sensorCount,
            CreatedAt = farm.CreatedAt,
            UpdatedAt = farm.UpdatedAt
        };
    }

    private static FarmValidationRangeDto MapRangeToDto(FarmValidationRange r) => new()
    {
        Id          = r.Id,
        FarmId      = r.FarmId,
        SensorType  = r.SensorType,
        MinNormal   = r.MinNormal,
        MaxNormal   = r.MaxNormal,
        WarningLow  = r.WarningLow,
        WarningHigh = r.WarningHigh,
        CriticalLow = r.CriticalLow,
        CriticalHigh = r.CriticalHigh,
        CreatedAt   = r.CreatedAt,
        UpdatedAt   = r.UpdatedAt
    };
}
