using MongoDB.Bson;
using MongoDB.Driver;

namespace AgriculturalMonitorSystem.Config;

public static class DatabaseIndexSetup
{
    public static async Task CreateIndexesAsync(IMongoDatabase database)
    {
        // ── Users ─────────────────────────────────────────────────────────────
        var users = database.GetCollection<BsonDocument>("Users");
        await users.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("email"),
            new CreateIndexOptions { Unique = true, Name = "idx_users_email_unique" }));
        await users.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("role"),
            new CreateIndexOptions { Name = "idx_users_role" }));

        // ── Farms ─────────────────────────────────────────────────────────────
        var farms = database.GetCollection<BsonDocument>("Farms");
        await farms.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("user_id"),
            new CreateIndexOptions { Name = "idx_farms_user_id" }));

        // ── Sensors ───────────────────────────────────────────────────────────
        var sensors = database.GetCollection<BsonDocument>("Sensors");
        await sensors.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("farm_id"),
            new CreateIndexOptions { Name = "idx_sensors_farm_id" }));
        await sensors.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("status"),
            new CreateIndexOptions { Name = "idx_sensors_status" }));

        // ── SensorReadings ────────────────────────────────────────────────────
        var readings = database.GetCollection<BsonDocument>("SensorReadings");
        await readings.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("farm_id").Descending("timestamp"),
            new CreateIndexOptions { Name = "idx_readings_farm_timestamp" }));
        await readings.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("sensor_id"),
            new CreateIndexOptions { Name = "idx_readings_sensor_id" }));

        // ── Alerts ────────────────────────────────────────────────────────────
        var alerts = database.GetCollection<BsonDocument>("Alerts");
        await alerts.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("farm_id"),
            new CreateIndexOptions { Name = "idx_alerts_farm_id" }));
        await alerts.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("is_resolved"),
            new CreateIndexOptions { Name = "idx_alerts_is_resolved" }));
        await alerts.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("timestamp"),
            new CreateIndexOptions { Name = "idx_alerts_timestamp" }));
    }
}
