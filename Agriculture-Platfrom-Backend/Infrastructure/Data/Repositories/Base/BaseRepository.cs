using MongoDB.Bson;
using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.Exceptions;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

/// <summary>
/// Generic base repository for MongoDB collections.
/// Feature repositories inherit this and extend with domain-specific queries.
/// </summary>
public abstract class BaseRepository<T> : IBaseRepository<T>
{
    protected readonly IMongoCollection<T> _collection;

    protected BaseRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", SafeParseId(id));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<List<T>> GetAllAsync()
        => await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();

    public virtual async Task InsertAsync(T entity)
        => await _collection.InsertOneAsync(entity);

    public virtual async Task InsertManyAsync(IEnumerable<T> entities)
        => await _collection.InsertManyAsync(entities);

    public virtual async Task<bool> UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", SafeParseId(id));
        var result = await _collection.ReplaceOneAsync(filter, entity);
        return result.ModifiedCount > 0;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", SafeParseId(id));
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", SafeParseId(id));
        return await _collection.CountDocumentsAsync(filter) > 0;
    }

    public virtual async Task<long> CountAsync()
        => await _collection.CountDocumentsAsync(Builders<T>.Filter.Empty);

    protected static ObjectId SafeParseId(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            throw new BadRequestException($"Invalid ID format: '{id}'.");
        return objectId;
    }
}
