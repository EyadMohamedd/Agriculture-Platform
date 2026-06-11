using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Admin;

public interface IFarmValidationRangeRepository : IBaseRepository<FarmValidationRange>
{
    Task<FarmValidationRange?> GetFarmValidationRangeAsync(string farmId, string sensorType);
    Task<List<FarmValidationRange>> GetFarmValidationRangesAsync(string farmId);
    Task UpsertFarmValidationRangeAsync(FarmValidationRange range);
    Task DeleteFarmValidationRangesByFarmIdAsync(string farmId);
    Task<bool> FarmValidationRangeExistsAsync(string farmId, string sensorType);
}
