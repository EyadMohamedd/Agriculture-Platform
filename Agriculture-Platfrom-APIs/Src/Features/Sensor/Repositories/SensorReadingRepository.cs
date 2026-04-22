using MongoDB.Driver;
using AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Models;
using AgriculturalMonitorSystem.Src.Shared.Repositories;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;

public class SensorReadingRepository : SharedRepository<SensorReading>, ISensorReadingRepository
{
    public SensorReadingRepository(IMongoDatabase database) : base(database, "SensorReadings") { }

    public async Task<PagedResult<SensorReading>> GetByFarmIdPagedAsync(string farmId, PaginationParams pagination)
    {
        var filter = Builders<SensorReading>.Filter.Eq(r => r.FarmId, farmId);
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
            "npk"          => readings.Where(r => r.NpkN.HasValue).Select(r => r.NpkN!.Value).ToList(),
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
