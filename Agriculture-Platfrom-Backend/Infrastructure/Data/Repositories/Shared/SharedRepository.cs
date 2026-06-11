using MongoDB.Driver;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Shared;

public abstract class SharedRepository<T> : BaseRepository<T>, ISharedRepository<T> where T : class
{
    protected SharedRepository(IMongoDatabase database, string collectionName) : base(database, collectionName) { }
}
