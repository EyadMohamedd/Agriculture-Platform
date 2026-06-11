using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Sensors;

public class SensorReadingRepository : BaseRepository<SensorReading>, ISensorReadingRepository
{
    public SensorReadingRepository(IMongoDatabase database) : base(database, "SensorReadings") { }

    public async Task<PagedResult<SensorReading>> GetByFarmIdPagedAsync(
        string farmId, PaginationParams pagination, string? sensorType = null,
        DateTime? from = null, DateTime? to = null)
    {
        var filter = Builders<SensorReading>.Filter.Eq(r => r.FarmId, farmId)
            & BuildSensorTypeFilter(sensorType)
            & BuildDateRangeFilter(from, to);

        var totalCount = await _collection.CountDocumentsAsync(filter);
        var sort = Builders<SensorReading>.Sort.Descending(r => r.Timestamp);

        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<SensorReading>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    private static FilterDefinition<SensorReading> BuildSensorTypeFilter(string? sensorType) => sensorType switch
    {
        "temperature"    => Builders<SensorReading>.Filter.Ne(r => r.Temperature, null),
        "ph"             => Builders<SensorReading>.Filter.Ne(r => r.SoilPh, null),
        "moisture"       => Builders<SensorReading>.Filter.Ne(r => r.SoilMoisture, null),
        "npk" or "npk_n" => Builders<SensorReading>.Filter.Ne(r => r.NpkN, null),
        "npk_p"          => Builders<SensorReading>.Filter.Ne(r => r.NpkP, null),
        "npk_k"          => Builders<SensorReading>.Filter.Ne(r => r.NpkK, null),
        "rainfall"       => Builders<SensorReading>.Filter.Ne(r => r.Rainfall, null),
        _                => Builders<SensorReading>.Filter.Empty
    };

    private static FilterDefinition<SensorReading> BuildDateRangeFilter(DateTime? from, DateTime? to)
    {
        var filter = Builders<SensorReading>.Filter.Empty;
        if (from.HasValue) filter &= Builders<SensorReading>.Filter.Gte(r => r.Timestamp, from.Value);
        if (to.HasValue)   filter &= Builders<SensorReading>.Filter.Lte(r => r.Timestamp, to.Value);
        return filter;
    }

    public async Task<List<SensorReading>> GetLatestByFarmIdAsync(string farmId, int limit = 10)
    {
        var filter = Builders<SensorReading>.Filter.Eq(r => r.FarmId, farmId);
        return await _collection.Find(filter)
            .Sort(Builders<SensorReading>.Sort.Descending(r => r.Timestamp))
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<SensorReading?> GetLatestBySensorIdAsync(string sensorId)
    {
        var filter = Builders<SensorReading>.Filter.Eq(r => r.SensorId, sensorId);
        return await _collection.Find(filter)
            .Sort(Builders<SensorReading>.Sort.Descending(r => r.Timestamp))
            .Limit(1)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<string, double>> GetStatisticsByFarmIdAsync(
        string farmId, string sensorType, DateTime from, DateTime to)
    {
        var filter = Builders<SensorReading>.Filter.And(
            Builders<SensorReading>.Filter.Eq(r => r.FarmId, farmId),
            Builders<SensorReading>.Filter.Gte(r => r.Timestamp, from),
            Builders<SensorReading>.Filter.Lte(r => r.Timestamp, to));

        var readings = await _collection.Find(filter).ToListAsync();

        var values = sensorType switch
        {
            "temperature"  => readings.Where(r => r.Temperature.HasValue).Select(r => r.Temperature!.Value).ToList(),
            "ph"           => readings.Where(r => r.SoilPh.HasValue).Select(r => r.SoilPh!.Value).ToList(),
            "moisture"     => readings.Where(r => r.SoilMoisture.HasValue).Select(r => r.SoilMoisture!.Value).ToList(),
            "npk" or "npk_n" => readings.Where(r => r.NpkN.HasValue).Select(r => r.NpkN!.Value).ToList(),
            "npk_p"        => readings.Where(r => r.NpkP.HasValue).Select(r => r.NpkP!.Value).ToList(),
            "npk_k"        => readings.Where(r => r.NpkK.HasValue).Select(r => r.NpkK!.Value).ToList(),
            "rainfall"     => readings.Where(r => r.Rainfall.HasValue).Select(r => r.Rainfall!.Value).ToList(),
            _              => []
        };

        if (values.Count == 0) return [];

        return new Dictionary<string, double>
        {
            { "min",   values.Min() },
            { "max",   values.Max() },
            { "avg",   values.Average() },
            { "count", values.Count }
        };
    }

    public async Task DeleteBySensorIdAsync(string sensorId)
    {
        var filter = Builders<SensorReading>.Filter.Eq(r => r.SensorId, sensorId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task DeleteByFarmIdAsync(string farmId)
    {
        var filter = Builders<SensorReading>.Filter.Eq(r => r.FarmId, farmId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task<long> CountBySensorIdAsync(string sensorId)
    {
        var filter = Builders<SensorReading>.Filter.Eq(r => r.SensorId, sensorId);
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<List<SensorReading>> GetRecentBySensorIdAsync(string sensorId, int hours = 1)
    {
        var since = DateTime.UtcNow.AddHours(-hours);
        var filter = Builders<SensorReading>.Filter.And(
            Builders<SensorReading>.Filter.Eq(r => r.SensorId, sensorId),
            Builders<SensorReading>.Filter.Gte(r => r.Timestamp, since));
        return await _collection.Find(filter).ToListAsync();
    }
}
