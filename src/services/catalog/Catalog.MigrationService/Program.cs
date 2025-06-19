using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.Logging;
using Catalog.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ApiDbInitializer>();

builder.AddServiceDefaults();

var migrationAssembly = typeof(IAssemblyMarker).Assembly;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("catalogdb"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(migrationAssembly.GetName().Name);
    }));
builder.EnrichNpgsqlDbContext<AppDbContext>(settings =>
    // Disable Aspire default retries as we're using a custom execution strategy
    settings.DisableRetry = true);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    logger.LogInformation("Starting database migration...");
    await app.RunAsync();
    logger.LogInformation("Database migration completed successfully.");
    Environment.Exit(0);
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Database migration failed.");
    Environment.Exit(1);
}
