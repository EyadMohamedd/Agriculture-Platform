# Agricultural Monitoring System — Claude Context

## Project Identity

- **Solution:** `Agriculture-Platfrom-APIs` (note the typo in the folder name — do not rename it)
- **Namespace root:** `AgriculturalMonitorSystem`
- **Runtime:** .NET 10.0.201, file-scoped namespace declarations throughout
- **Database:** MongoDB 7+ via `MongoDB.Driver` 3.1.0
- **Working directory:** `d:/Agriculture system backend/Agriculture-Platfrom-APIs/`
- **Build check:** `dotnet build` from the project root — must always end `0 Error(s)` before finishing any task

---

## Architecture

### Pattern: Feature Folder (Vertical Slice)

Each feature lives entirely inside `Src/Features/{Feature}/`:

```
Controllers/   — one controller class
Services/      — IXxxService + XxxService
Repositories/  — IXxxRepository + XxxRepository
Models/
  Entities/    — MongoDB document class (BSON attributes)
  DTOs/        — request/response shapes
Validators/    — FluentValidation rules
```

Shared infrastructure lives in `Src/Shared/`. Never put domain logic there.

### Features

| Feature | Route prefix | Notes |
|---|---|---|
| Auth | `/api/auth` | Registration, login, forgot/reset password, logout, delete account |
| User | `/api/users` | Profile get/update, change password |
| Farm | `/api/farms` | CRUD + per-farm validation range overrides |
| Sensor | `/api/sensors` | Readings list, latest dashboard, statistics |
| Alert | `/api/alerts` | View only — alerts are auto-generated, read-only |
| Admin | `/api/admin` | User management, system validation ranges |

---

## Middleware Pipeline (ORDER IS CRITICAL)

Defined in `Program.cs`. Do not reorder.

```
UseCors
→ ErrorHandlingMiddleware   (catches all exceptions, maps to HTTP status)
→ RequestLoggingMiddleware  (Serilog timing)
→ AuthMiddleware            (validates JWT, sets HttpContext.Items["UserId"] / ["UserRole"])
→ RoleMiddleware            (enforces [AuthorizeRole] attribute)
→ FarmOwnershipMiddleware   (enforces [RequireFarmOwnership] attribute)
→ MapControllers
```

### Custom Attributes

```csharp
[AuthorizeRole(RoleConstants.Admin)]                   // class or method level
[AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
[RequireFarmOwnership("route", "farmId")]              // source: "route"|"query", path: param name
[RequireFarmOwnership("route", "id")]
```

- `AuthMiddleware` skips endpoints with no `[AuthorizeRole]` attribute — they are public.
- `FarmOwnershipMiddleware` — Admin role **always bypasses** the check. Farmers must own the farm.
- Ownership results are cached via `IMemoryCache` with a 5-minute TTL (`ResourceOwnershipService`).

---

## Authentication

- **Stateless JWT** — no ASP.NET Identity, no built-in JWT middleware.
- `JwtHelper` (singleton) signs and validates tokens.
- `PasswordHasher` (singleton) wraps BCrypt.Net-Next with work factor 11.
- Token lifetime configured in `appsettings.json → JwtSettings.ExpiryHours` (default 24).
- Token claims: `sub` = userId, `role` = user role.
- Logout is client-side only — the backend endpoint is a no-op (stateless).

---

## Roles

```csharp
RoleConstants.Admin   = "Admin"
RoleConstants.Farmer  = "Farmer"
```

Default role for all new registrations is `Farmer`. Only Admin can change roles.
The last Admin account cannot be deleted or demoted (enforced in both `AdminService` and `AuthService`).

---

## MongoDB Collections & Entity Mapping

All entity string IDs are MongoDB ObjectIds:

```csharp
[BsonId]
[BsonRepresentation(BsonType.ObjectId)]
public string Id { get; set; } = string.Empty;
```

Foreign-key fields (e.g., `FarmId`, `UserId`) also use `[BsonRepresentation(BsonType.ObjectId)]`.

BSON field names use `snake_case` via `[BsonElement("field_name")]`.
Nullable fields use `[BsonIgnoreIfNull]`.

| Collection | Entity class | Key fields |
|---|---|---|
| `Users` | `User` | `email` (unique index), `role`, `password_hash` |
| `Farms` | `Farm` | `user_id`, `name`, `location`, `crop_type` (optional) |
| `Sensors` | `Sensor` | `farm_id`, `sensor_type`, `status` |
| `SensorReadings` | `SensorReading` | `sensor_id`, `farm_id`, `timestamp` |
| `Alerts` | `Alert` | `sensor_id`, `farm_id`, `user_id`, `type`, `is_resolved` |
| `ValidationRanges` | `ValidationRange` | `sensor_type` (unique) |
| `FarmValidationRanges` | `FarmValidationRange` | `farm_id` + `sensor_type` (unique compound) |

Indexes are created on startup by `DatabaseIndexSetup.CreateIndexesAsync(db)`.

---

## Base Repository

`SharedRepository<T>` in `Src/Shared/Repositories/SharedRepository.cs` provides:
`GetByIdAsync`, `GetAllAsync`, `InsertAsync`, `InsertManyAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsAsync`, `CountAsync`

Feature repositories extend it with domain queries. `UpdateAsync` does a **full document replace** (`ReplaceOneAsync`) — fetch first, mutate, then call update.

---

## Known C# Namespace Collision — CRITICAL

Within `...Feature.{Name}.Repositories` and `...Feature.{Name}.Services` namespaces, the simple name `Farm`, `Alert`, or `Sensor` resolves to the **parent namespace segment**, not the entity class, making `using` directives for those entity types useless.

**Fix:** Use non-conflicting type aliases — do not use the same name as the namespace segment:

```csharp
// WRONG — alias name matches namespace segment, still resolves to namespace
using Farm = AgriculturalMonitorSystem.Src.Features.Farm.Models.Entities.Farm;

// CORRECT
using FarmEntity   = AgriculturalMonitorSystem.Src.Features.Farm.Models.Entities.Farm;
using AlertEntity  = AgriculturalMonitorSystem.Src.Features.Alert.Models.Entities.Alert;
using SensorEntity = AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities.Sensor;
```

This alias pattern is already applied in all affected files. Maintain it whenever editing those files.

---

## Sensor Types & Sensor Auto-Creation

When a farm is created, **5 sensors are automatically created** in `FarmService.CreateFarmAsync`:

| Sensor `SensorType` | Reading field populated | Alert evaluated as |
|---|---|---|
| `temperature` | `Temperature` | `temperature` |
| `ph` | `SoilPh` | `ph` |
| `moisture` | `SoilMoisture` | `moisture` |
| `npk` | `NpkN`, `NpkP`, `NpkK` | `npk_n`, `npk_p`, `npk_k` (3 separate alert checks) |
| `rainfall` | `Rainfall` | `rainfall` |

The NPK sensor is the only one that generates **three sub-readings** from a single `SensorReading` document and triggers three independent alert evaluations.

Sensors have no public creation/deletion endpoints — they are managed entirely through farm lifecycle events.

---

## Alert System

- Alerts are **read-only** from the API perspective (no resolve, no create, no delete via API).
- Generated automatically by `AlertService.ProcessReadingForAlertsAsync`, called by `SensorSimulationService` after each reading.
- **Deduplication:** If an unresolved alert of the same `(sensorId, alertType)` exists within the last hour, the existing alert's `Timestamp` is updated instead of inserting a new one.

### Severity Levels (evaluated in order)

```
Critical : value < criticalLow  OR value > criticalHigh
High     : value < warningLow   OR value > warningHigh
Medium   : value < minNormal    OR value > maxNormal
(Normal  : no alert)
```

Alert `Type` field format: `{sensorType}_{condition}`, e.g. `temperature_critical_high`, `npk_n_warning_low`.

---

## Hierarchical Validation Ranges

Alert thresholds are looked up in this priority order in `AlertService.ProcessReadingForAlertsAsync`:

1. **Farm override** (`FarmValidationRanges` collection) — keyed by `(farmId, sensorType)`
2. **System default exact match** (`ValidationRanges` collection) — keyed by `sensorType`
3. **System default base type fallback** — strips suffix: `npk_n` → tries `npk` next

If no range is found at any level, the alert check is skipped (logged at Debug).

Threshold ordering enforced by validators: `criticalLow < warningLow < minNormal < maxNormal < warningHigh < criticalHigh`

Valid `sensorType` values for farm ranges: `temperature`, `ph`, `moisture`, `npk_n`, `npk_p`, `npk_k`, `rainfall`

---

## Cascade Delete Rules

Implemented in `Src/Shared/Services/DeleteService.cs`:

| Delete target | Cascade behaviour |
|---|---|
| **User** (`force=false`) | Blocks if user has any farms — returns `400` |
| **User** (`force=true`) | Cascade-deletes all user's farms first (each triggers farm cascade), then deletes user |
| **Farm** | Per-sensor: delete readings + alerts → delete all sensors → delete farm-level readings/alerts → delete farm validation ranges → delete farm |
| **Sensor** | Delete readings + alerts for that sensor → delete sensor |

`DeleteAccountAsync` in `AuthService` always calls `DeleteUserAsync(userId, force: true)` — user accounts delete cleanly without pre-deleting farms.
`AdminService.DeleteUserAsync` calls `DeleteUserAsync(userId)` without force — Admin must delete farms first (or use the cascade API).

---

## Sensor Simulation

`SensorSimulationService` is a `BackgroundService` (singleton) registered in `Program.cs`:

```csharp
builder.Services.AddSingleton<SensorSimulationService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<SensorSimulationService>());
```

- **Starts automatically** when the application starts — no API call required.
- **No start/stop control** — runs continuously until the app shuts down.
- Interval: `appsettings.json → SimulationSettings.IntervalSeconds` (default 300 = 5 min). Set to `10` for local testing.
- `AnomalyProbability` (default `0.1`): probability each reading is anomalous (uses `AnomalyRanges` instead of `NormalRanges`).
- Uses `IServiceProvider.CreateScope()` per cycle to resolve scoped services safely from a singleton.
- Uses `appsettings.json → SimulationSettings.NormalRanges` / `AnomalyRanges` dictionaries for value generation — these are independent of the alert threshold ranges.

---

## Startup Auto-Seeding

`RunStartupTasksAsync` in `Program.cs` runs on every startup:

1. Creates MongoDB indexes (idempotent)
2. Seeds 7 system-default `ValidationRanges` if the collection is empty
3. Creates admin account `admin@agrisystem.com` / `Admin@123` if it does not exist

Default admin credentials (development only): `admin@agrisystem.com` / `Admin@123`

---

## Exception Hierarchy → HTTP Status Codes

All in `Src/Shared/Exceptions/`. Thrown in service layer, caught and serialised by `ErrorHandlingMiddleware`.

| Exception class | HTTP status |
|---|---|
| `NotFoundException` | 404 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `ConflictException` | 409 |
| `BadRequestException` | 400 |
| Any other `Exception` | 500 |

---

## API Response Envelope

Every response uses `ApiResponse<T>`:

```json
{ "success": true,  "message": "...", "data": {...}, "errors": null }
{ "success": false, "message": "...", "data": null,  "errors": ["..."] }
```

Paginated data wraps `data` in `PagedResult<T>`:
```json
{ "items": [...], "totalCount": 42, "page": 1, "pageSize": 20 }
```

`PaginationParams` query params: `page` (default 1), `pageSize` (default 20, max 100), `sortBy`, `sortOrder` (`asc`|`desc`).

---

## Configuration Reference (`appsettings.json`)

```json
{
  "MongoDbSettings":    { "ConnectionString": "...", "DatabaseName": "AgriculturalMonitorDB" },
  "JwtSettings":        { "Secret": "...(min 32 chars)...", "ExpiryHours": 24, "Issuer": "AgriSystem", "Audience": "AgriSystemClient" },
  "SimulationSettings": {
    "IntervalSeconds": 300,
    "AnomalyProbability": 0.1,
    "NormalRanges":  { "temperature": {"Min":15,"Max":35}, ... },
    "AnomalyRanges": { "temperature": {"Min":0, "Max":50}, ... }
  }
}
```

Logs are written to `Logs/agri-YYYYMMDD.log` (rolling daily) via Serilog. Also output to console.

---

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `MongoDB.Driver` | 3.1.0 | Database driver |
| `BCrypt.Net-Next` | 4.0.3 | Password hashing (work factor 11) |
| `FluentValidation.AspNetCore` | 11.3.0 | Request validation |
| `Serilog.AspNetCore` | 9.0.0 | Structured logging |
| `Serilog.Sinks.Console` | 6.0.0 | Console sink |
| `Serilog.Sinks.File` | 6.0.0 | Rolling file sink |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.0 | JWT support (referenced, not used for middleware) |
| `Microsoft.Extensions.Caching.Memory` | 10.0.0 | In-process cache for ownership checks |

> `NU1510` build warning for `Microsoft.Extensions.Caching.Memory` is harmless — it is included transitively in .NET 10 and the explicit reference is redundant but not harmful.

---

## Coding Conventions

- **File-scoped namespaces** everywhere: `namespace Foo.Bar;` not `namespace Foo.Bar { }`
- **Collection expressions** for array literals: `["a", "b"]` not `new[] { "a", "b" }`
- **Null-coalescing throw**: `?? throw new NotFoundException(...)` — no null checks with if-statements
- **Async/await throughout** — no `.Result` or `.Wait()`
- **UpdateAsync is full-replace**: always fetch the entity first, mutate the C# object, then call `UpdateAsync`
- **No Swagger/OpenAPI** — not configured, not expected
- **No ASP.NET Identity** — custom JWT middleware only
- **CORS** is open (`AllowAnyOrigin`) for development — tighten before production

---

## Files That Do Not Exist (intentionally removed)

- `ResolveAlertDto.cs` — removed; alerts are read-only
- `ResolveAlertValidator.cs` — removed; alerts are read-only

---

## Seed Script

`SeedData/seed.js` — run with mongosh to manually reset a dev database:

```bash
mongosh mongodb://localhost:27017/AgriculturalMonitorDB SeedData/seed.js
```

The `password_hash` in the script is a placeholder. On first app startup the real BCrypt hash is generated automatically. Only use the script to reset `ValidationRanges` or recreate indexes after a manual wipe.
