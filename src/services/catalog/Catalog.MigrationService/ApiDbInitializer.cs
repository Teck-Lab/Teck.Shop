using System.Diagnostics;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using Catalog.Infrastructure.Persistence;

namespace Catalog.MigrationService
{
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDbInitializer"/> class.
        /// </summary>
        /// <param name="serviceProvider">The application's service provider.</param>
        /// <param name="hostEnvironment">The hosting environment.</param>
        /// <param name="hostApplicationLifetime">The application lifetime manager.</param>
    public class ApiDbInitializer(
        IServiceProvider serviceProvider,
        IHostEnvironment hostEnvironment,
        IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
    {
        private readonly ActivitySource _activitySource = new(hostEnvironment.ApplicationName);

        /// <summary>
        /// Executes the background service to ensure the database exists and applies migrations.
        /// </summary>
        /// <param name="stoppingToken">A token to monitor for cancellation requests.</param>
                protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                {
                    using var activity = _activitySource.StartActivity(hostEnvironment.ApplicationName, ActivityKind.Client);
                    var logger = serviceProvider.GetRequiredService<ILogger<ApiDbInitializer>>();
                    try
                    {
                        logger.LogInformation("Ensuring database exists...");
                        using var scope = serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
                        await EnsureDatabaseAsync(dbContext, stoppingToken);
                        logger.LogInformation("Database exists or was created.");
        
                        logger.LogInformation("Running migrations...");
                        await RunMigrationAsync(dbContext, stoppingToken);
                        logger.LogInformation("Migrations applied successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogCritical(ex, "Migration failed.");
                        activity?.AddException(ex);
                        // Do not rethrow, just stop application after logging
                    }
        
                    hostApplicationLifetime.StopApplication();
                }

        private static async Task EnsureDatabaseAsync(AppDbContext dbContext, CancellationToken cancellationToken)
        {
            var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Create the database if it does not exist.
                // Do this first so there is then a database to start a transaction against.
                if (!await dbCreator.ExistsAsync(cancellationToken))
                {
                    await dbCreator.CreateAsync(cancellationToken);
                }
            });
        }

        private static async Task RunMigrationAsync(AppDbContext dbContext, CancellationToken cancellationToken)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Run migration in a transaction to avoid partial migration if it fails.
                await dbContext.Database.MigrateAsync(cancellationToken);
            });
        }
    }
}
