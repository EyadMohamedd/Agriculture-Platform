using MongoDB.Bson;
using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Farms;

public class FarmRepository : BaseRepository<Farm>, IFarmRepository
{
    public FarmRepository(IMongoDatabase database) : base(database, "Farms") { }

    public async Task<List<Farm>> GetByUserIdAsync(string userId)
    {
        var filter = Builders<Farm>.Filter.Eq(f => f.UserId, userId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<PagedResult<Farm>> GetAllPagedAsync(PaginationParams pagination)
        => await GetPagedAsync(Builders<Farm>.Filter.Empty, pagination);

    public async Task<PagedResult<Farm>> GetByUserIdPagedAsync(string userId, PaginationParams pagination)
    {
        var filter = Builders<Farm>.Filter.Eq(f => f.UserId, userId);
        return await GetPagedAsync(filter, pagination);
    }

    public async Task<bool> BelongsToUserAsync(string farmId, string userId)
    {
        if (!ObjectId.TryParse(farmId, out var oid)) return false;
        var filter = Builders<Farm>.Filter.And(
            Builders<Farm>.Filter.Eq("_id", oid),
            Builders<Farm>.Filter.Eq(f => f.UserId, userId));
        return await _collection.CountDocumentsAsync(filter) > 0;
    }

    private async Task<PagedResult<Farm>> GetPagedAsync(FilterDefinition<Farm> filter, PaginationParams pagination)
    {
        var totalCount = await _collection.CountDocumentsAsync(filter);
        var sort = pagination.SortOrder == "asc"
            ? Builders<Farm>.Sort.Ascending(pagination.SortBy ?? "created_at")
            : Builders<Farm>.Sort.Descending(pagination.SortBy ?? "created_at");

        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<Farm>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
