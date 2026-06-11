using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Shared;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Admin;

public interface IAdminRepository : ISharedRepository<ValidationRange>
{
    // ── System-default validation ranges ─────────────────────────────────────
    Task<ValidationRange?> GetValidationRangeByTypeAsync(string sensorType);
    Task<List<ValidationRange>> GetAllValidationRangesAsync();
    Task<bool> ValidationRangeExistsForTypeAsync(string sensorType);

    /// <summary>Seeds the default validation ranges on first startup if not already present.</summary>
    Task SeedDefaultValidationRangesAsync();

    // ── Farm-specific validation range overrides ──────────────────────────────
    /// <summary>Returns the farm-specific override for a sensor type, or null if none set.</summary>
    Task<FarmValidationRange?> GetFarmValidationRangeAsync(string farmId, string sensorType);

    Task<List<FarmValidationRange>> GetFarmValidationRangesAsync(string farmId);

    /// <summary>Inserts or replaces the farm-specific override for the given sensor type.</summary>
    Task UpsertFarmValidationRangeAsync(FarmValidationRange range);

    Task<FarmValidationRange?> GetFarmValidationRangeByIdAsync(string id);

    Task<bool> DeleteFarmValidationRangeAsync(string id);

    /// <summary>Cascade-deletes all custom ranges for a farm when the farm is deleted.</summary>
    Task DeleteFarmValidationRangesByFarmIdAsync(string farmId);

    Task<bool> FarmValidationRangeExistsAsync(string farmId, string sensorType);
}
