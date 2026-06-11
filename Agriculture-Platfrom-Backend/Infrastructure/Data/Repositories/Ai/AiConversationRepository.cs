using MongoDB.Driver;
using AgriculturalMonitorSystem.Application.DomainModels;
using AgriculturalMonitorSystem.Shared.Models;
using AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Base;

namespace AgriculturalMonitorSystem.Infrastructure.Data.Repositories.Ai;

public class AiConversationRepository : BaseRepository<AiConversation>, IAiConversationRepository
{
    public AiConversationRepository(IMongoDatabase database) : base(database, "AiConversations") { }

    public async Task<PagedResult<AiConversation>> GetByUserIdPagedAsync(string userId, PaginationParams pagination)
    {
        var filter = Builders<AiConversation>.Filter.Eq(c => c.UserId, userId);
        var totalCount = await _collection.CountDocumentsAsync(filter);
        var sort = Builders<AiConversation>.Sort.Descending(c => c.Timestamp);

        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<AiConversation>
        {
            Items      = items,
            TotalCount = (int)totalCount,
            Page       = pagination.Page,
            PageSize   = pagination.PageSize
        };
    }

    public async Task DeleteByFarmIdAsync(string farmId)
    {
        var filter = Builders<AiConversation>.Filter.Eq(c => c.FarmId, farmId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task DeleteByUserIdAsync(string userId)
    {
        var filter = Builders<AiConversation>.Filter.Eq(c => c.UserId, userId);
        await _collection.DeleteManyAsync(filter);
    }
}
