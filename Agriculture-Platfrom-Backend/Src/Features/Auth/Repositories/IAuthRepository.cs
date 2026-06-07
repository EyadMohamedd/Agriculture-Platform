using AgriculturalMonitorSystem.Src.Features.Auth.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Repositories;

public interface IAuthRepository : ISharedRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task CreatePasswordResetTokenAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token);
    Task MarkTokenAsUsedAsync(string tokenId);
    Task<bool> IsLastAdminAsync(string userId);
    Task<PagedResult<User>> GetUsersPagedAsync(PaginationParams pagination);
    Task DeletePasswordResetTokensByUserIdAsync(string userId);
}
