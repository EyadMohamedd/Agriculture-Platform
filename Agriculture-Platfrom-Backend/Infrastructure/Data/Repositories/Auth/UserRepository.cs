using MongoDB.Bson;
using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Auth;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(IMongoDatabase database) : base(database, "Users") { }

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

    public async Task<bool> IsLastAdminAsync(string userId)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Role, "Admin"),
            Builders<User>.Filter.Ne("_id", ObjectId.Parse(userId)));
        return await _collection.CountDocumentsAsync(filter) == 0;
    }

    public async Task<PagedResult<User>> GetUsersPagedAsync(PaginationParams pagination)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Role, "Farmer");
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
