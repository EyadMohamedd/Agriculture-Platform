using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Admin.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Admin.Repositories;
using AgriculturalMonitorSystem.Src.Features.Auth.Repositories;
using AgriculturalMonitorSystem.Src.Features.Farm.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Farm.Repositories;
using AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;
using SensorEntity = AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities.Sensor;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Exceptions;
using AgriculturalMonitorSystem.Src.Shared.Interfaces;
using AgriculturalMonitorSystem.Src.Shared.Models;
using FarmEntity = AgriculturalMonitorSystem.Src.Features.Farm.Models.Entities.Farm;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Services;

public class FarmService : IFarmService
{
    private readonly IFarmRepository _farmRepository;
    private readonly IAuthRepository _authRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IDeleteService _deleteService;
    private readonly ILogger<FarmService> _logger;

    // The 5 sensor types auto-created per farm
    private static readonly string[] SensorTypes = ["temperature", "ph", "moisture", "npk", "rainfall"];

    public FarmService(
        IFarmRepository farmRepository,
        IAuthRepository authRepository,
        ISensorRepository sensorRepository,
        IAdminRepository adminRepository,
        IDeleteService deleteService,
        ILogger<FarmService> logger)
    {
        _farmRepository = farmRepository;
        _authRepository = authRepository;
        _sensorRepository = sensorRepository;
        _adminRepository = adminRepository;
        _deleteService = deleteService;
        _logger = logger;
    }

    public async Task<FarmResponseDto> CreateFarmAsync(CreateFarmDto dto, string userId)
    {
        var user = await _authRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        // Both Farmers and Admins can create farms
        var farm = new FarmEntity
        {
            UserId = userId,
            Name = dto.Name.Trim(),
            Location = dto.Location,
            CropType = string.IsNullOrWhiteSpace(dto.CropType) ? null : dto.CropType.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _farmRepository.InsertAsync(farm);

        // ── AUTOMATICALLY create 5 sensors for this farm ──────────────────────
        var sensors = SensorTypes.Select(type => new SensorEntity
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
        PagedResult<FarmEntity> result;

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

        // Cascades: sensors → readings → alerts → farm
        await _deleteService.DeleteFarmAsync(farmId);
        _logger.LogInformation("Farm {FarmId} deleted (cascade) by user {UserId}", farmId, userId);
    }

    // ── Farm-specific validation range overrides ──────────────────────────────

    public async Task<List<FarmValidationRangeDto>> GetFarmValidationRangesAsync(
        string farmId, string userId, string userRole)
    {
        await EnsureFarmAccessAsync(farmId, userId, userRole);
        var ranges = await _adminRepository.GetFarmValidationRangesAsync(farmId);
        return ranges.Select(MapRangeToDto).ToList();
    }

    public async Task<FarmValidationRangeDto> UpsertFarmValidationRangeAsync(
        string farmId, FarmValidationRangeDto dto, string userId, string userRole)
    {
        await EnsureFarmAccessAsync(farmId, userId, userRole);

        var existing = await _adminRepository.GetFarmValidationRangeAsync(farmId, dto.SensorType);
        var range = existing ?? new FarmValidationRange { FarmId = farmId, SensorType = dto.SensorType, CreatedAt = DateTime.UtcNow };

        range.MinNormal  = dto.MinNormal;
        range.MaxNormal  = dto.MaxNormal;
        range.WarningLow = dto.WarningLow;
        range.WarningHigh = dto.WarningHigh;
        range.CriticalLow = dto.CriticalLow;
        range.CriticalHigh = dto.CriticalHigh;
        range.UpdatedAt = DateTime.UtcNow;

        await _adminRepository.UpsertFarmValidationRangeAsync(range);

        // Re-fetch to get assigned Id
        var saved = await _adminRepository.GetFarmValidationRangeAsync(farmId, dto.SensorType);
        return MapRangeToDto(saved!);
    }

    public async Task DeleteFarmValidationRangeAsync(
        string farmId, string rangeId, string userId, string userRole)
    {
        await EnsureFarmAccessAsync(farmId, userId, userRole);

        var range = await _adminRepository.GetFarmValidationRangeByIdAsync(rangeId)
            ?? throw new NotFoundException("Farm validation range not found.");

        if (range.FarmId != farmId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);

        await _adminRepository.DeleteFarmValidationRangeAsync(rangeId);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task EnsureFarmAccessAsync(string farmId, string userId, string userRole)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId)
            ?? throw new NotFoundException(ErrorMessages.FarmNotFound);
        if (userRole != RoleConstants.Admin && farm.UserId != userId)
            throw new ForbiddenException(ErrorMessages.FarmAccessDenied);
    }

    private async Task<FarmResponseDto> MapToResponseDtoAsync(FarmEntity farm)
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
