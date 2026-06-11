using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Admin;

public interface IValidationRangeRepository : IBaseRepository<ValidationRange>
{
    Task<ValidationRange?> GetValidationRangeByTypeAsync(string sensorType);
    Task<List<ValidationRange>> GetAllValidationRangesAsync();
    Task<bool> ValidationRangeExistsForTypeAsync(string sensorType);
    Task SeedDefaultValidationRangesAsync();
}
