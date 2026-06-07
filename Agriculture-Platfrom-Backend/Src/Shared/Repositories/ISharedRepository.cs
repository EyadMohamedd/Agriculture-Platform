namespace AgriculturalMonitorSystem.Src.Shared.Repositories;

public interface ISharedRepository<T>
{
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
    Task InsertAsync(T entity);
    Task InsertManyAsync(IEnumerable<T> entities);
    Task<bool> UpdateAsync(string id, T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<long> CountAsync();
}
