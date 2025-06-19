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
using Teck.Shop.SharedKernel.Persistence.Database.EFCore.Interceptors;
using Microsoft.AspNetCore.Http;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.RabbitMq;
using MassTransit.Mediator;

namespace Catalog.IntegrationTests.Shared
{
    public abstract class BaseCacheTestFixture<TContext> : IAsyncLifetime where TContext : BaseDbContext
    {
        protected FusionCache Cache = null!;
        protected IUnitOfWork UnitOfWork = null!;
        protected readonly PostgreSqlContainer DbContainer;
        protected readonly RabbitMqContainer RabbitMqContainer;
        protected TContext DbContext = null!;
        protected SoftDeleteInterceptor SoftDeleteInterceptor = null!;
        protected AuditingInterceptor AuditingInterceptor = null!;
        protected DomainEventInterceptor DomainEventInterceptor = null!;
        protected IntegrationEventInterceptor IntegrationEventInterceptor = null!;
        protected IServiceProvider ServiceProvider = null!;
        protected IPublishEndpoint PublishEndpoint = null!;

        protected BaseCacheTestFixture()
        {
            DbContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
            RabbitMqContainer = RabbitMqTestContainerFactory.Create();
        }

        public virtual async ValueTask InitializeAsync()
        {
            await DbContainer.StartAsync();
            await RabbitMqContainer.StartAsync();
            var httpContextAccessor = new HttpContextAccessor();
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(RabbitMqContainer.GetConnectionString());
                });
            });
            services.AddMediator();
            ServiceProvider = services.BuildServiceProvider();
            PublishEndpoint = ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var mediator = ServiceProvider.GetRequiredService<IScopedMediator>();
            SoftDeleteInterceptor = new SoftDeleteInterceptor(httpContextAccessor);
            AuditingInterceptor = new AuditingInterceptor(httpContextAccessor);
            DomainEventInterceptor = new DomainEventInterceptor(mediator);

            var options = new DbContextOptionsBuilder<TContext>()
                .UseNpgsql(DbContainer.GetConnectionString())
                .AddInterceptors(SoftDeleteInterceptor, AuditingInterceptor, DomainEventInterceptor)
                .Options;
            DbContext = CreateDbContext(options);
            await DbContext.Database.EnsureCreatedAsync();
            await SeedAsync();

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
            await RabbitMqContainer.DisposeAsync();
            Cache?.Dispose();
        }

        protected abstract TContext CreateDbContext(DbContextOptions<TContext> options);

        /// <summary>
        /// Optional: Override to seed data before each test.
        /// </summary>
        protected virtual Task SeedAsync() => Task.CompletedTask;
    }
}
