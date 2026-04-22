using MongoDB.Bson;
using MongoDB.Driver;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;
using FarmEntity = AgriculturalMonitorSystem.Src.Features.Farm.Models.Entities.Farm;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Repositories;

public class FarmRepository : SharedRepository<FarmEntity>, IFarmRepository
{
    public FarmRepository(IMongoDatabase database) : base(database, "Farms") { }

    public async Task<List<FarmEntity>> GetByUserIdAsync(string userId)
    {
        var filter = Builders<FarmEntity>.Filter.Eq(f => f.UserId, userId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<PagedResult<FarmEntity>> GetAllPagedAsync(PaginationParams pagination)
        => await GetPagedAsync(Builders<FarmEntity>.Filter.Empty, pagination);

    public async Task<PagedResult<FarmEntity>> GetByUserIdPagedAsync(string userId, PaginationParams pagination)
    {
        var filter = Builders<FarmEntity>.Filter.Eq(f => f.UserId, userId);
        return await GetPagedAsync(filter, pagination);
    }

    public async Task<bool> BelongsToUserAsync(string farmId, string userId)
    {
        if (!ObjectId.TryParse(farmId, out var oid)) return false;
        var filter = Builders<FarmEntity>.Filter.And(
            Builders<FarmEntity>.Filter.Eq("_id", oid),
            Builders<FarmEntity>.Filter.Eq(f => f.UserId, userId));
        return await _collection.CountDocumentsAsync(filter) > 0;
    }

    private async Task<PagedResult<FarmEntity>> GetPagedAsync(FilterDefinition<FarmEntity> filter, PaginationParams pagination)
    {
        var totalCount = await _collection.CountDocumentsAsync(filter);
        var sort = pagination.SortOrder == "asc"
            ? Builders<FarmEntity>.Sort.Ascending(pagination.SortBy ?? "created_at")
            : Builders<FarmEntity>.Sort.Descending(pagination.SortBy ?? "created_at");

        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<FarmEntity>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
