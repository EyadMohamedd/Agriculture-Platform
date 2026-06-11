using MongoDB.Bson;
using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Admin;

public class FarmValidationRangeRepository : BaseRepository<FarmValidationRange>, IFarmValidationRangeRepository
{
    public FarmValidationRangeRepository(IMongoDatabase database) : base(database, "FarmValidationRanges") { }

    public async Task<FarmValidationRange?> GetFarmValidationRangeAsync(string farmId, string sensorType)
    {
        var filter = Builders<FarmValidationRange>.Filter.And(
            Builders<FarmValidationRange>.Filter.Eq(r => r.FarmId, farmId),
            Builders<FarmValidationRange>.Filter.Eq(r => r.SensorType, sensorType));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<FarmValidationRange>> GetFarmValidationRangesAsync(string farmId)
    {
        var filter = Builders<FarmValidationRange>.Filter.Eq(r => r.FarmId, farmId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task UpsertFarmValidationRangeAsync(FarmValidationRange range)
    {
        if (string.IsNullOrEmpty(range.Id))
        {
            await _collection.InsertOneAsync(range);
        }
        else
        {
            var filter = Builders<FarmValidationRange>.Filter.Eq("_id", ObjectId.Parse(range.Id));
            await _collection.ReplaceOneAsync(filter, range, new ReplaceOptions { IsUpsert = true });
        }
    }

    public async Task DeleteFarmValidationRangesByFarmIdAsync(string farmId)
    {
        var filter = Builders<FarmValidationRange>.Filter.Eq(r => r.FarmId, farmId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task<bool> FarmValidationRangeExistsAsync(string farmId, string sensorType)
    {
        var filter = Builders<FarmValidationRange>.Filter.And(
            Builders<FarmValidationRange>.Filter.Eq(r => r.FarmId, farmId),
            Builders<FarmValidationRange>.Filter.Eq(r => r.SensorType, sensorType));
        return await _collection.CountDocumentsAsync(filter) > 0;
    }
}
