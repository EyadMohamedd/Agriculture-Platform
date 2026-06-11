using MongoDB.Bson;
using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Auth;

public class PasswordResetTokenRepository : BaseRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(IMongoDatabase database) : base(database, "PasswordResetTokens") { }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        var filter = Builders<PasswordResetToken>.Filter.Eq(t => t.Token, token);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task MarkTokenAsUsedAsync(string tokenId)
    {
        var filter = Builders<PasswordResetToken>.Filter.Eq("_id", SafeParseId(tokenId));
        var update = Builders<PasswordResetToken>.Update.Set(t => t.IsUsed, true);
        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteTokensByUserIdAsync(string userId)
    {
        var filter = Builders<PasswordResetToken>.Filter.Eq(t => t.UserId, userId);
        await _collection.DeleteManyAsync(filter);
    }
}
