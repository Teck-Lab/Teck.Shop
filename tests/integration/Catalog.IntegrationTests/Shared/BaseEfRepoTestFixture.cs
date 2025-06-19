using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore.Interceptors;
using Microsoft.AspNetCore.Http;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Mediator;

namespace Catalog.IntegrationTests.Shared
{
    public abstract class BaseEfRepoTestFixture<TContext, TUnitOfWork> : IAsyncLifetime where TContext : DbContext
    {
        protected readonly SharedTestcontainersFixture SharedFixture;
        protected TContext DbContext = null!;
        protected TUnitOfWork UnitOfWork = default!;
        protected SoftDeleteInterceptor SoftDeleteInterceptor = null!;
        protected AuditingInterceptor AuditingInterceptor = null!;
        protected DomainEventInterceptor DomainEventInterceptor = null!;
        protected IntegrationEventInterceptor IntegrationEventInterceptor = null!;
        protected IServiceProvider ServiceProvider = null!;

        protected BaseEfRepoTestFixture(SharedTestcontainersFixture sharedFixture)
        {
            SharedFixture = sharedFixture;
        }

        public virtual async ValueTask InitializeAsync()
        {
            // Containers are already started by the shared fixture
            var httpContextAccessor = new HttpContextAccessor();
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(SharedFixture.RabbitMqContainer.GetConnectionString());
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
                .UseNpgsql(SharedFixture.DbContainer.GetConnectionString())
                .AddInterceptors(SoftDeleteInterceptor, AuditingInterceptor, DomainEventInterceptor)
                .Options;
            DbContext = CreateDbContext(options);
            UnitOfWork = CreateUnitOfWork(DbContext);
            await DbContext.Database.EnsureCreatedAsync();
            await SeedAsync();
        }

        public virtual async ValueTask DisposeAsync()
        {
            // Do NOT dispose containers here; handled by shared fixture
        }

        protected abstract TContext CreateDbContext(DbContextOptions<TContext> options);

        protected abstract TUnitOfWork CreateUnitOfWork(TContext context);

        /// <summary>
        /// Optional: Override to seed data before each test.
        /// </summary>
        protected virtual Task SeedAsync() => Task.CompletedTask;
    }
}
