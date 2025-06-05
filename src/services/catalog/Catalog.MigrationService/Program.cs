using Catalog.Infrastructure.Persistence;
using Catalog.MigrationService;
using Catalog.Infrastructure;
using Catalog.MigrationService;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ApiDbInitializer>();

builder.AddServiceDefaults();

var migrationAssembly = typeof(IAssemblyMarker).Assembly;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("catalogdb"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(migrationAssembly.FullName);
    }));
builder.EnrichNpgsqlDbContext<AppDbContext>(settings =>
    // Disable Aspire default retries as we're using a custom execution strategy
    settings.DisableRetry = true);

var app = builder.Build();

app.Run();
