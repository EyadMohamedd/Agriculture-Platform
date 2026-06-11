using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Admin;

public class ValidationRangeRepository : BaseRepository<ValidationRange>, IValidationRangeRepository
{
    public ValidationRangeRepository(IMongoDatabase database) : base(database, "ValidationRanges") { }

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
}
