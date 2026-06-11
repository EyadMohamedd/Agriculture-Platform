using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Shared;

public interface ISharedRepository<T> : IBaseRepository<T> where T : class
{
}
