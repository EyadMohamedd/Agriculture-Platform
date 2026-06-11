using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Auth;

public interface IPasswordResetTokenRepository : IBaseRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task MarkTokenAsUsedAsync(string tokenId);
    Task DeleteTokensByUserIdAsync(string userId);
}
