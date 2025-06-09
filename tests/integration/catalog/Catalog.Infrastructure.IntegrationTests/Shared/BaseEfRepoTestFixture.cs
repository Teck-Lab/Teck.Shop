using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Catalog.Infrastructure.IntegrationTests.Shared
{
    public abstract class BaseEfRepoTestFixture<TContext> : IAsyncLifetime where TContext : DbContext
    {
        protected readonly PostgreSqlContainer DbContainer;
        protected TContext DbContext = null!;

        protected BaseEfRepoTestFixture()
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
        }

        public virtual async ValueTask DisposeAsync()
        {
            await DbContainer.DisposeAsync();
        }

        protected abstract TContext CreateDbContext(DbContextOptions<TContext> options);

        /// <summary>
        /// Optional: Override to seed data before each test.
        /// </summary>
        protected virtual Task SeedAsync() => Task.CompletedTask;
    }
}
