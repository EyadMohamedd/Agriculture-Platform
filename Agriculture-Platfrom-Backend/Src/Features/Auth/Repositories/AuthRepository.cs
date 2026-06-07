using MongoDB.Bson;
using MongoDB.Driver;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Repositories;

public class AuthRepository : SharedRepository<User>, IAuthRepository
{
    private readonly IMongoCollection<PasswordResetToken> _resetTokens;

    public AuthRepository(IMongoDatabase database) : base(database, "Users")
    {
        _resetTokens = database.GetCollection<PasswordResetToken>("PasswordResetTokens");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        return await _collection.CountDocumentsAsync(filter) > 0;
    }

    public async Task CreatePasswordResetTokenAsync(PasswordResetToken token)
        => await _resetTokens.InsertOneAsync(token);

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token)
    {
        var filter = Builders<PasswordResetToken>.Filter.Eq(t => t.Token, token);
        return await _resetTokens.Find(filter).FirstOrDefaultAsync();
    }

    public async Task MarkTokenAsUsedAsync(string tokenId)
    {
        var filter = Builders<PasswordResetToken>.Filter.Eq("_id", SafeParseId(tokenId));
        var update = Builders<PasswordResetToken>.Update.Set(t => t.IsUsed, true);
        await _resetTokens.UpdateOneAsync(filter, update);
    }

    public async Task DeletePasswordResetTokensByUserIdAsync(string userId)
    {
        var filter = Builders<PasswordResetToken>.Filter.Eq(t => t.UserId, userId);
        await _resetTokens.DeleteManyAsync(filter);
    }

    public async Task<bool> IsLastAdminAsync(string userId)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Role, RoleConstants.Admin),
            Builders<User>.Filter.Ne("_id", ObjectId.Parse(userId)));
        return await _collection.CountDocumentsAsync(filter) == 0;
    }

    public async Task<PagedResult<User>> GetUsersPagedAsync(PaginationParams pagination)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Role, RoleConstants.Farmer);
        var totalCount = await _collection.CountDocumentsAsync(filter);

        var sort = pagination.SortOrder == "asc"
            ? Builders<User>.Sort.Ascending(pagination.SortBy ?? "created_at")
            : Builders<User>.Sort.Descending(pagination.SortBy ?? "created_at");

        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
