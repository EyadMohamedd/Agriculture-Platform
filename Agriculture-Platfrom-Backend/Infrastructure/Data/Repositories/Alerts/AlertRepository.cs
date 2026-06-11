using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Alerts;

public class AlertRepository : BaseRepository<Alert>, IAlertRepository
{
    public AlertRepository(IMongoDatabase database) : base(database, "Alerts") { }

    public async Task<PagedResult<Alert>> GetAllPagedAsync(PaginationParams pagination)
        => await GetPagedAsync(Builders<Alert>.Filter.Empty, pagination);

    public async Task<PagedResult<Alert>> GetByUserIdPagedAsync(
        string userId, PaginationParams pagination, string? farmId = null, string? severity = null)
    {
        var filter = Builders<Alert>.Filter.Eq(a => a.UserId, userId);
        if (!string.IsNullOrEmpty(farmId))
            filter &= Builders<Alert>.Filter.Eq(a => a.FarmId, farmId);
        if (!string.IsNullOrEmpty(severity))
            filter &= Builders<Alert>.Filter.Eq(a => a.Severity, severity);
        return await GetPagedAsync(filter, pagination);
    }

    public async Task<Alert?> GetUnresolvedBySensorAndTypeAsync(string sensorId, string alertType, DateTime since)
    {
        var filter = Builders<Alert>.Filter.And(
            Builders<Alert>.Filter.Eq(a => a.SensorId, sensorId),
            Builders<Alert>.Filter.Eq(a => a.Type, alertType),
            Builders<Alert>.Filter.Eq(a => a.IsResolved, false),
            Builders<Alert>.Filter.Gte(a => a.Timestamp, since));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<Alert>> GetActiveByFarmIdAsync(string farmId)
    {
        var filter = Builders<Alert>.Filter.And(
            Builders<Alert>.Filter.Eq(a => a.FarmId, farmId),
            Builders<Alert>.Filter.Eq(a => a.IsResolved, false));
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task ResolveBySensorTypePrefixAndSeveritiesAsync(
        string sensorId, string alertTypePrefix, IEnumerable<string> severities)
    {
        var filter = Builders<Alert>.Filter.And(
            Builders<Alert>.Filter.Eq(a => a.SensorId, sensorId),
            Builders<Alert>.Filter.Eq(a => a.IsResolved, false),
            Builders<Alert>.Filter.Regex(a => a.Type, new MongoDB.Bson.BsonRegularExpression($"^{alertTypePrefix}_")),
            Builders<Alert>.Filter.In(a => a.Severity, severities));
        var update = Builders<Alert>.Update
            .Set(a => a.IsResolved, true)
            .Set(a => a.ResolvedAt, DateTime.UtcNow);
        await _collection.UpdateManyAsync(filter, update);
    }

    public async Task DeleteByFarmIdAsync(string farmId)
    {
        var filter = Builders<Alert>.Filter.Eq(a => a.FarmId, farmId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task DeleteBySensorIdAsync(string sensorId)
    {
        var filter = Builders<Alert>.Filter.Eq(a => a.SensorId, sensorId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task<string?> GetFarmIdByAlertIdAsync(string alertId)
    {
        var alert = await GetByIdAsync(alertId);
        return alert?.FarmId;
    }

    private async Task<PagedResult<Alert>> GetPagedAsync(FilterDefinition<Alert> filter, PaginationParams pagination)
    {
        var totalCount = await _collection.CountDocumentsAsync(filter);
        var sort = pagination.SortOrder == "asc"
            ? Builders<Alert>.Sort.Ascending(pagination.SortBy ?? "timestamp")
            : Builders<Alert>.Sort.Descending(pagination.SortBy ?? "timestamp");

        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<Alert>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
