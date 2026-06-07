using MongoDB.Driver;
using AgriculturalMonitorSystem.Src.Shared.Repositories;
using SensorEntity = AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities.Sensor;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;

public class SensorRepository : SharedRepository<SensorEntity>, ISensorRepository
{
    public SensorRepository(IMongoDatabase database) : base(database, "Sensors") { }

    public async Task<List<SensorEntity>> GetByFarmIdAsync(string farmId)
    {
        var filter = Builders<SensorEntity>.Filter.Eq(s => s.FarmId, farmId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<SensorEntity>> GetActiveSensorsAsync()
    {
        var filter = Builders<SensorEntity>.Filter.Eq(s => s.Status, "active");
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<long> CountByFarmIdAsync(string farmId)
    {
        var filter = Builders<SensorEntity>.Filter.Eq(s => s.FarmId, farmId);
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task DeleteByFarmIdAsync(string farmId)
    {
        var filter = Builders<SensorEntity>.Filter.Eq(s => s.FarmId, farmId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task<string?> GetFarmIdBySensorIdAsync(string sensorId)
    {
        var sensor = await GetByIdAsync(sensorId);
        return sensor?.FarmId;
    }
}
