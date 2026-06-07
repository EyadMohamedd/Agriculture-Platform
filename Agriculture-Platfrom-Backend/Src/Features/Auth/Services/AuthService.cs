using AgriculturalMonitorSystem.Src.Features.Auth.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Auth.Repositories;
using AgriculturalMonitorSystem.Src.Features.Auth.Utils;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Exceptions;
using AgriculturalMonitorSystem.Src.Shared.Interfaces;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly JwtHelper _jwtHelper;
    private readonly PasswordHasher _passwordHasher;
    private readonly IDeleteService _deleteService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAuthRepository authRepository,
        JwtHelper jwtHelper,
        PasswordHasher passwordHasher,
        IDeleteService deleteService,
        ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _jwtHelper = jwtHelper;
        _passwordHasher = passwordHasher;
        _deleteService = deleteService;
        _logger = logger;
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        if (await _authRepository.EmailExistsAsync(dto.Email.ToLowerInvariant()))
            throw new ConflictException(ErrorMessages.EmailAlreadyExists);

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.ToLowerInvariant(),
            Phone = dto.Phone,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = RoleConstants.Farmer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _authRepository.InsertAsync(user);
        _logger.LogInformation("New user registered: {Email}", user.Email);
        return user;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _authRepository.GetByEmailAsync(dto.Email.ToLowerInvariant())
            ?? throw new UnauthorizedException(ErrorMessages.InvalidCredentials);

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);

        _logger.LogInformation("User logged in: {Email}", user.Email);

        return new LoginResponseDto
        {
            Token = _jwtHelper.GenerateToken(user),
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = _jwtHelper.GetExpiryTime()
        };
    }

    public async Task<User> GetProfileAsync(string userId)
        => await _authRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

    public async Task<User> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _authRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        if (dto.Name != null) user.Name = dto.Name.Trim();
        if (dto.Phone != null) user.Phone = dto.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.UpdateAsync(userId, user);
        return user;
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _authRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BadRequestException(ErrorMessages.CurrentPasswordIncorrect);

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _authRepository.UpdateAsync(userId, user);
        _logger.LogInformation("Password changed for user: {UserId}", userId);
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _authRepository.GetByEmailAsync(dto.Email.ToLowerInvariant())
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        // Q1: "What is your farm's registration number?" → stored as Phone
        var phoneMatch = string.Equals(
            dto.FarmRegistrationNumber.Trim(),
            user.Phone.Trim(),
            StringComparison.OrdinalIgnoreCase);

        // Q2: "What is your username?" → stored as Name
        var nameMatch = string.Equals(
            dto.Username.Trim(),
            user.Name.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (!phoneMatch || !nameMatch)
            throw new UnauthorizedException("The answers provided do not match our records.");

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"), // 64-char hex token
            Expiry = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };

        await _authRepository.CreatePasswordResetTokenAsync(resetToken);
        _logger.LogInformation("Password reset token issued via security questions for user {UserId}", user.Id);

        return resetToken.Token;
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var tokenRecord = await _authRepository.GetPasswordResetTokenAsync(dto.Token)
            ?? throw new BadRequestException(ErrorMessages.InvalidToken);

        if (tokenRecord.IsUsed)
            throw new BadRequestException(ErrorMessages.TokenAlreadyUsed);

        if (tokenRecord.Expiry < DateTime.UtcNow)
            throw new BadRequestException(ErrorMessages.TokenExpired);

        var user = await _authRepository.GetByIdAsync(tokenRecord.UserId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _authRepository.UpdateAsync(user.Id, user);
        await _authRepository.MarkTokenAsUsedAsync(tokenRecord.Id);
        _logger.LogInformation("Password reset completed for user: {UserId}", user.Id);
    }

    public async Task DeleteAccountAsync(string userId, string requestingUserId, string requestingUserRole)
    {
        if (requestingUserRole != RoleConstants.Admin && userId != requestingUserId)
            throw new ForbiddenException(ErrorMessages.ForbiddenAccess);

        var user = await _authRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);

        if (user.Role == RoleConstants.Admin && await _authRepository.IsLastAdminAsync(userId))
            throw new BadRequestException(ErrorMessages.LastAdminProtected);

        // Cascade-delete all farms (sensors, readings, alerts, ranges) before removing the user
        await _deleteService.DeleteUserAsync(userId, force: true);
        _logger.LogInformation("Account deleted: {UserId} by {RequestingUserId}", userId, requestingUserId);
    }
}
