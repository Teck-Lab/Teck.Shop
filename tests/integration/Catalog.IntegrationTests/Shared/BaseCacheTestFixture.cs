using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using ZiggyCreatures.Caching.Fusion;
using Xunit;
using Catalog.Infrastructure.Persistence;
using ZiggyCreatures.Caching.Fusion.Backplane.Memory;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;

namespace Catalog.IntegrationTests.Shared
{
    public abstract class BaseCacheTestFixture<TContext> : IAsyncLifetime where TContext : BaseDbContext
    {
        protected FusionCache Cache = null!;
        protected IUnitOfWork UnitOfWork = null!;
        protected readonly PostgreSqlContainer DbContainer;
        protected TContext DbContext = null!;

        protected BaseCacheTestFixture()
        {
            DbContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }

        public virtual async ValueTask InitializeAsync()
        {
            await DbContainer.StartAsync();
            var options = new DbContextOptionsBuilder<TContext>()
                .UseNpgsql(DbContainer.GetConnectionString())
                .Options;
            DbContext = CreateDbContext(options);
            await DbContext.Database.EnsureCreatedAsync();
            await SeedAsync();

            UnitOfWork = new UnitOfWork<TContext>(DbContext);
            var cacheOptions = new FusionCacheOptions
            {
                // You can tweak options here if needed
            };
            Cache = new FusionCache(cacheOptions);

            // Add in-memory backplane for multi-instance cache sync (optional)
            var backplane = new MemoryBackplane(new MemoryBackplaneOptions());
            Cache.SetupBackplane(backplane);

            
        }

        public virtual async ValueTask DisposeAsync()
        {
            await DbContainer.DisposeAsync();
            Cache?.Dispose();
        }

        protected abstract TContext CreateDbContext(DbContextOptions<TContext> options);

        /// <summary>
        /// Optional: Override to seed data before each test.
        /// </summary>
        protected virtual Task SeedAsync() => Task.CompletedTask;
    }
}
