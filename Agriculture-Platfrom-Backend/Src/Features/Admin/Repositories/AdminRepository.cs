using MongoDB.Bson;
using MongoDB.Driver;
using AgriculturalMonitorSystem.Src.Features.Admin.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Repositories;

namespace AgriculturalMonitorSystem.Src.Features.Admin.Repositories;

public class AdminRepository : SharedRepository<ValidationRange>, IAdminRepository
{
    private readonly IMongoCollection<FarmValidationRange> _farmRanges;

    public AdminRepository(IMongoDatabase database) : base(database, "ValidationRanges")
    {
        _farmRanges = database.GetCollection<FarmValidationRange>("FarmValidationRanges");
    }

    // ── System-default validation ranges ─────────────────────────────────────

    public async Task<ValidationRange?> GetValidationRangeByTypeAsync(string sensorType)
    {
        var filter = Builders<ValidationRange>.Filter.Eq(v => v.SensorType, sensorType);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<ValidationRange>> GetAllValidationRangesAsync()
        => await _collection.Find(Builders<ValidationRange>.Filter.Empty).ToListAsync();

    public async Task<bool> ValidationRangeExistsForTypeAsync(string sensorType)
    {
        var filter = Builders<ValidationRange>.Filter.Eq(v => v.SensorType, sensorType);
        return await _collection.CountDocumentsAsync(filter) > 0;
    }

    public async Task SeedDefaultValidationRangesAsync()
    {
        var count = await _collection.CountDocumentsAsync(Builders<ValidationRange>.Filter.Empty);
        if (count > 0) return;

        var defaults = new List<ValidationRange>
        {
            new() { SensorType = "temperature", MinNormal = 15,  MaxNormal = 35,  WarningLow = 10,  WarningHigh = 40,  CriticalLow = 0,   CriticalHigh = 50  },
            new() { SensorType = "ph",          MinNormal = 6.0, MaxNormal = 7.5, WarningLow = 5.5, WarningHigh = 8.0, CriticalLow = 4.0, CriticalHigh = 9.0 },
            new() { SensorType = "moisture",    MinNormal = 40,  MaxNormal = 70,  WarningLow = 30,  WarningHigh = 80,  CriticalLow = 20,  CriticalHigh = 90  },
            new() { SensorType = "npk_n",       MinNormal = 50,  MaxNormal = 150, WarningLow = 30,  WarningHigh = 180, CriticalLow = 10,  CriticalHigh = 200 },
            new() { SensorType = "npk_p",       MinNormal = 20,  MaxNormal = 80,  WarningLow = 10,  WarningHigh = 100, CriticalLow = 5,   CriticalHigh = 120 },
            new() { SensorType = "npk_k",       MinNormal = 30,  MaxNormal = 120, WarningLow = 15,  WarningHigh = 150, CriticalLow = 10,  CriticalHigh = 180 },
            new() { SensorType = "rainfall",    MinNormal = 40,  MaxNormal = 70,  WarningLow = 30,  WarningHigh = 80,  CriticalLow = 20,  CriticalHigh = 90  }
        };

        await _collection.InsertManyAsync(defaults);
    }

    // ── Farm-specific validation range overrides ──────────────────────────────

    public async Task<FarmValidationRange?> GetFarmValidationRangeAsync(string farmId, string sensorType)
    {
        var filter = Builders<FarmValidationRange>.Filter.And(
            Builders<FarmValidationRange>.Filter.Eq(r => r.FarmId, farmId),
            Builders<FarmValidationRange>.Filter.Eq(r => r.SensorType, sensorType));
        return await _farmRanges.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<FarmValidationRange>> GetFarmValidationRangesAsync(string farmId)
    {
        var filter = Builders<FarmValidationRange>.Filter.Eq(r => r.FarmId, farmId);
        return await _farmRanges.Find(filter).ToListAsync();
    }

    public async Task UpsertFarmValidationRangeAsync(FarmValidationRange range)
    {
        if (string.IsNullOrEmpty(range.Id))
        {
            // New record — insert
            await _farmRanges.InsertOneAsync(range);
        }
        else
        {
            // Existing record — replace
            var filter = Builders<FarmValidationRange>.Filter.Eq("_id", ObjectId.Parse(range.Id));
            await _farmRanges.ReplaceOneAsync(filter, range, new ReplaceOptions { IsUpsert = true });
        }
    }

    public async Task<FarmValidationRange?> GetFarmValidationRangeByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var oid)) return null;
        var filter = Builders<FarmValidationRange>.Filter.Eq("_id", oid);
        return await _farmRanges.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteFarmValidationRangeAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var oid)) return false;
        var filter = Builders<FarmValidationRange>.Filter.Eq("_id", oid);
        var result = await _farmRanges.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task DeleteFarmValidationRangesByFarmIdAsync(string farmId)
    {
        var filter = Builders<FarmValidationRange>.Filter.Eq(r => r.FarmId, farmId);
        await _farmRanges.DeleteManyAsync(filter);
    }

    public async Task<bool> FarmValidationRangeExistsAsync(string farmId, string sensorType)
    {
        var filter = Builders<FarmValidationRange>.Filter.And(
            Builders<FarmValidationRange>.Filter.Eq(r => r.FarmId, farmId),
            Builders<FarmValidationRange>.Filter.Eq(r => r.SensorType, sensorType));
        return await _farmRanges.CountDocumentsAsync(filter) > 0;
    }
}
