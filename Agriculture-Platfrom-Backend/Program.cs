using FluentValidation;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using AgriculturalMonitorSystem.Config;
using AgriculturalMonitorSystem.Src.Features.Admin.Controllers;
using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Admin.Repositories;
using AgriculturalMonitorSystem.Src.Features.Admin.Services;
using AgriculturalMonitorSystem.Src.Features.Admin.Validators;
using AgriculturalMonitorSystem.Src.Features.Alert.Repositories;
using AgriculturalMonitorSystem.Src.Features.Alert.Services;
using AgriculturalMonitorSystem.Src.Features.Alert.Validators;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.Entities;
using AgriculturalMonitorSystem.Src.Features.Auth.Repositories;
using AgriculturalMonitorSystem.Src.Features.Auth.Services;
using AgriculturalMonitorSystem.Src.Features.Auth.Utils;
using AgriculturalMonitorSystem.Src.Features.Auth.Validators;
using AgriculturalMonitorSystem.Src.Features.Farm.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Farm.Repositories;
using AgriculturalMonitorSystem.Src.Features.Farm.Services;
using AgriculturalMonitorSystem.Src.Features.Farm.Validators;
using AgriculturalMonitorSystem.Src.Features.Ai.Repositories;
using AgriculturalMonitorSystem.Src.Features.Ai.Services;
using AgriculturalMonitorSystem.Src.Features.Sensor.Repositories;
using AgriculturalMonitorSystem.Src.Features.Sensor.Services;
using AgriculturalMonitorSystem.Src.Shared.BackgroundServices;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Interfaces;
using AgriculturalMonitorSystem.Src.Shared.Middleware;
using AgriculturalMonitorSystem.Src.Shared.Services;

// ── Serilog early configuration ───────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/agri-.log", rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Agricultural Monitor System...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ── Configuration objects ─────────────────────────────────────────────────
    var mongoSettings      = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;
    var jwtSettings        = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
    var simulationSettings = builder.Configuration.GetSection("SimulationSettings").Get<SimulationSettings>()!;
    var aiSettings         = builder.Configuration.GetSection("AiSettings").Get<AiSettings>()!;

    builder.Services.AddSingleton(mongoSettings);
    builder.Services.AddSingleton(jwtSettings);
    builder.Services.AddSingleton(simulationSettings);
    builder.Services.AddSingleton(aiSettings);

    // ── MongoDB ───────────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
    builder.Services.AddSingleton(sp =>
        sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

    // ── Memory cache (used by ResourceOwnershipService) ───────────────────────
    builder.Services.AddMemoryCache();

    // ── Repositories (Scoped — one per HTTP request) ──────────────────────────
    builder.Services.AddScoped<IAuthRepository,         AuthRepository>();
    builder.Services.AddScoped<IFarmRepository,         FarmRepository>();
    builder.Services.AddScoped<ISensorRepository,       SensorRepository>();
    builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
    builder.Services.AddScoped<IAlertRepository,        AlertRepository>();
    builder.Services.AddScoped<IAdminRepository,        AdminRepository>();
    builder.Services.AddScoped<IAiRepository,           AiRepository>();

    // ── Shared services ───────────────────────────────────────────────────────
    builder.Services.AddScoped<IReferenceChecker,       ReferenceChecker>();
    builder.Services.AddScoped<IResourceOwnershipService, ResourceOwnershipService>();
    builder.Services.AddScoped<IDeleteService,          DeleteService>();
    builder.Services.AddScoped<EnvironmentContextBuilder>();
    builder.Services.AddHttpClient<AiHttpClient>();

    // ── Auth utilities ────────────────────────────────────────────────────────
    builder.Services.AddSingleton<JwtHelper>();
    builder.Services.AddSingleton<PasswordHasher>();

    // ── Feature services (Scoped) ─────────────────────────────────────────────
    builder.Services.AddScoped<IAuthService,    AuthService>();
    builder.Services.AddScoped<IFarmService,    FarmService>();
    builder.Services.AddScoped<ISensorService,  SensorService>();
    builder.Services.AddScoped<IAlertService,   AlertService>();
    builder.Services.AddScoped<IAdminService,   AdminService>();
    builder.Services.AddScoped<IAiService,      AiService>();

    // ── Simulation background service (Singleton + HostedService) ─────────────
    builder.Services.AddSingleton<CsvReadingProvider>();
    builder.Services.AddSingleton<SensorSimulationService>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<SensorSimulationService>());

    // ── FluentValidation ──────────────────────────────────────────────────────
    builder.Services.AddScoped<IValidator<RegisterDto>,          RegisterValidator>();
    builder.Services.AddScoped<IValidator<LoginDto>,             LoginValidator>();
    builder.Services.AddScoped<IValidator<ChangePasswordDto>,    ChangePasswordValidator>();
    builder.Services.AddScoped<IValidator<CreateFarmDto>,        CreateFarmValidator>();
    builder.Services.AddScoped<IValidator<UpdateFarmDto>,        UpdateFarmValidator>();
    builder.Services.AddScoped<IValidator<ValidationRangeDto>,   ValidationRangeValidator>();
    builder.Services.AddScoped<IValidator<FarmValidationRangeDto>, FarmValidationRangeValidator>();

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ── CORS (open for development — tighten in production) ───────────────────
    builder.Services.AddCors(opts =>
        opts.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Startup tasks (indexes + seed data) ──────────────────────────────────
    await RunStartupTasksAsync(app);

    // ── Middleware pipeline (ORDER IS CRITICAL — mirrors Express.js .use() order)
    app.UseCors();

    app.UseMiddleware<ErrorHandlingMiddleware>();   // 1. Catch all exceptions
    app.UseMiddleware<RequestLoggingMiddleware>();  // 2. Log every request
    app.UseMiddleware<AuthMiddleware>();            // 3. Validate JWT, set user context
    app.UseMiddleware<RoleMiddleware>();            // 4. Enforce [AuthorizeRole]
    app.UseMiddleware<FarmOwnershipMiddleware>();   // 5. Enforce [RequireFarmOwnership]

    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ── Startup helper ────────────────────────────────────────────────────────────

static async Task RunStartupTasksAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

    // 1. Create MongoDB indexes
    await DatabaseIndexSetup.CreateIndexesAsync(db);
    Log.Information("MongoDB indexes verified/created");

    // 2. Seed system-default validation ranges (no-op if already seeded)
    var adminRepo = scope.ServiceProvider.GetRequiredService<IAdminRepository>();
    await adminRepo.SeedDefaultValidationRangesAsync();
    Log.Information("Validation range defaults verified");

    // 3. Seed default Admin user if no admin account exists
    var authRepo = scope.ServiceProvider.GetRequiredService<IAuthRepository>();
    var hasher   = scope.ServiceProvider.GetRequiredService<PasswordHasher>();

    if (!await authRepo.EmailExistsAsync("admin@agrisystem.com"))
    {
        await authRepo.InsertAsync(new User
        {
            Name         = "System Admin",
            Email        = "admin@agrisystem.com",
            Phone        = "+1234567890",
            PasswordHash = hasher.Hash("Admin@123"),
            Role         = RoleConstants.Admin,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        });
        Log.Information("Default admin account created: admin@agrisystem.com / Admin@123");
    }
}
