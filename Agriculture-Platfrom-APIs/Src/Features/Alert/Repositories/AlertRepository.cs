using MongoDB.Driver;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;
using AlertEntity = AgriculturalMonitorSystem.Src.Features.Alert.Models.Entities.Alert;

namespace AgriculturalMonitorSystem.Src.Features.Alert.Repositories;

public class AlertRepository : SharedRepository<AlertEntity>, IAlertRepository
{
    public AlertRepository(IMongoDatabase database) : base(database, "Alerts") { }

    public async Task<PagedResult<AlertEntity>> GetAllPagedAsync(PaginationParams pagination)
        => await GetPagedAsync(Builders<AlertEntity>.Filter.Empty, pagination);

    public async Task<PagedResult<AlertEntity>> GetByUserIdPagedAsync(
        string userId, PaginationParams pagination, string? farmId = null, string? severity = null)
    {
        var filter = Builders<AlertEntity>.Filter.Eq(a => a.UserId, userId);
        if (!string.IsNullOrEmpty(farmId))
            filter &= Builders<AlertEntity>.Filter.Eq(a => a.FarmId, farmId);
        if (!string.IsNullOrEmpty(severity))
            filter &= Builders<AlertEntity>.Filter.Eq(a => a.Severity, severity);
        return await GetPagedAsync(filter, pagination);
    }

    public async Task<AlertEntity?> GetUnresolvedBySensorAndTypeAsync(string sensorId, string alertType, DateTime since)
    {
        var filter = Builders<AlertEntity>.Filter.And(
            Builders<AlertEntity>.Filter.Eq(a => a.SensorId, sensorId),
            Builders<AlertEntity>.Filter.Eq(a => a.Type, alertType),
            Builders<AlertEntity>.Filter.Eq(a => a.IsResolved, false),
            Builders<AlertEntity>.Filter.Gte(a => a.Timestamp, since));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<AlertEntity>> GetActiveByFarmIdAsync(string farmId)
    {
        var filter = Builders<AlertEntity>.Filter.And(
            Builders<AlertEntity>.Filter.Eq(a => a.FarmId, farmId),
            Builders<AlertEntity>.Filter.Eq(a => a.IsResolved, false));
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task ResolveBySensorTypePrefixAndSeveritiesAsync(
        string sensorId, string alertTypePrefix, IEnumerable<string> severities)
    {
        var filter = Builders<AlertEntity>.Filter.And(
            Builders<AlertEntity>.Filter.Eq(a => a.SensorId, sensorId),
            Builders<AlertEntity>.Filter.Eq(a => a.IsResolved, false),
            Builders<AlertEntity>.Filter.Regex(a => a.Type, new MongoDB.Bson.BsonRegularExpression($"^{alertTypePrefix}_")),
            Builders<AlertEntity>.Filter.In(a => a.Severity, severities));
        var update = Builders<AlertEntity>.Update
            .Set(a => a.IsResolved, true)
            .Set(a => a.ResolvedAt, DateTime.UtcNow);
        await _collection.UpdateManyAsync(filter, update);
    }

    public async Task DeleteByFarmIdAsync(string farmId)
    {
        var filter = Builders<AlertEntity>.Filter.Eq(a => a.FarmId, farmId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task DeleteBySensorIdAsync(string sensorId)
    {
        var filter = Builders<AlertEntity>.Filter.Eq(a => a.SensorId, sensorId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task<string?> GetFarmIdByAlertIdAsync(string alertId)
    {
        var alert = await GetByIdAsync(alertId);
        return alert?.FarmId;
    }

    private async Task<PagedResult<AlertEntity>> GetPagedAsync(FilterDefinition<AlertEntity> filter, PaginationParams pagination)
    {
        var totalCount = await _collection.CountDocumentsAsync(filter);
        var sort = pagination.SortOrder == "asc"
            ? Builders<AlertEntity>.Sort.Ascending(pagination.SortBy ?? "timestamp")
            : Builders<AlertEntity>.Sort.Descending(pagination.SortBy ?? "timestamp");

        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<AlertEntity>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
