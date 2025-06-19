using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore.Interceptors;
using Microsoft.AspNetCore.Http;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.RabbitMq;
using MassTransit.Mediator;

namespace Catalog.IntegrationTests.Shared
{
    public abstract class BaseEfRepoTestFixture<TContext, TUnitOfWork> : IAsyncLifetime where TContext : DbContext
    {
        protected readonly PostgreSqlContainer DbContainer;
        protected readonly RabbitMqContainer RabbitMqContainer;
        protected TContext DbContext = null!;
        protected TUnitOfWork UnitOfWork = default!;
        protected SoftDeleteInterceptor SoftDeleteInterceptor = null!;
        protected AuditingInterceptor AuditingInterceptor = null!;
        protected DomainEventInterceptor DomainEventInterceptor = null!;
        protected IntegrationEventInterceptor IntegrationEventInterceptor = null!;
        protected IServiceProvider ServiceProvider = null!;

        protected BaseEfRepoTestFixture()
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
            var publishEndpoint = ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var mediator = ServiceProvider.GetRequiredService<IScopedMediator>();
            SoftDeleteInterceptor = new SoftDeleteInterceptor(httpContextAccessor);
            AuditingInterceptor = new AuditingInterceptor(httpContextAccessor);
            DomainEventInterceptor = new DomainEventInterceptor(mediator);

            var options = new DbContextOptionsBuilder<TContext>()
                .UseNpgsql(DbContainer.GetConnectionString())
                .AddInterceptors(SoftDeleteInterceptor, AuditingInterceptor, DomainEventInterceptor)
                .Options;
            DbContext = CreateDbContext(options);
            UnitOfWork = CreateUnitOfWork(DbContext);
            await DbContext.Database.EnsureCreatedAsync();
            await SeedAsync();
        }

        public virtual async ValueTask DisposeAsync()
        {
            await DbContainer.DisposeAsync();
            await RabbitMqContainer.DisposeAsync();
        }

        protected abstract TContext CreateDbContext(DbContextOptions<TContext> options);

        protected abstract TUnitOfWork CreateUnitOfWork(TContext context);

        /// <summary>
        /// Optional: Override to seed data before each test.
        /// </summary>
        protected virtual Task SeedAsync() => Task.CompletedTask;
    }
}
