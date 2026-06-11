using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Sensors;

public class SensorRepository : BaseRepository<Sensor>, ISensorRepository
{
    public SensorRepository(IMongoDatabase database) : base(database, "Sensors") { }

    public async Task<List<Sensor>> GetByFarmIdAsync(string farmId)
    {
        var filter = Builders<Sensor>.Filter.Eq(s => s.FarmId, farmId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<Sensor>> GetActiveSensorsAsync()
    {
        var filter = Builders<Sensor>.Filter.Eq(s => s.Status, "active");
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<long> CountByFarmIdAsync(string farmId)
    {
        var filter = Builders<Sensor>.Filter.Eq(s => s.FarmId, farmId);
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task DeleteByFarmIdAsync(string farmId)
    {
        var filter = Builders<Sensor>.Filter.Eq(s => s.FarmId, farmId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task<string?> GetFarmIdBySensorIdAsync(string sensorId)
    {
        var sensor = await GetByIdAsync(sensorId);
        return sensor?.FarmId;
    }
}
