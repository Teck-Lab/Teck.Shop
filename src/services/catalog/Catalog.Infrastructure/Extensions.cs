using System.Reflection;
using Catalog.Application.EventHandlers.DomainEvents;
using Catalog.Application.EventHandlers.IntegrationEvents;
using Catalog.Infrastructure.Persistence;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Scrutor;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Core.Events;
using Teck.Shop.SharedKernel.Core.Exceptions;
using Teck.Shop.SharedKernel.Infrastructure.Auth;
using Teck.Shop.SharedKernel.Persistence.Database;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore.Dispatchers;
namespace Catalog.Infrastructure
{
    /// <summary>
    /// The extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Add catalog infrastructure.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="applicationAssembly"></param>
        public static void AddCatalogInfrastructure(this WebApplicationBuilder builder, Assembly applicationAssembly)
        {
            Assembly dbContextAssembly = typeof(AppDbContext).Assembly;

            KeycloakAuthenticationOptions keycloakOptions = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>() ?? throw new ConfigurationMissingException("Keycloak");

            string postgresConnectionString = builder.Configuration.GetConnectionString("catalogdb") ?? throw new ConfigurationMissingException("Database");
            string rabbitmqConnectionString = builder.Configuration.GetConnectionString("rabbitmq") ?? throw new ConfigurationMissingException("RabbitMq");

            builder.Services.AddKeycloak(builder.Configuration, builder.Environment, keycloakOptions);

            builder.AddCustomDbContext<AppDbContext>(dbContextAssembly, postgresConnectionString);

            builder.Services.AddMediator(consumer =>
            {
                consumer.AddConsumer<BrandCreatedDomainEventConsumer>();
            });
            builder.Services.AddMassTransit(config =>
            {
                config.AddConsumer<BrandCreatedIntegrationEventConsumer>();
                config.AddEntityFrameworkOutbox<AppDbContext>(option =>
                {
                    option.UsePostgres();
                    option.UseBusOutbox();
                });
                
                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Publish<DomainEvent>(message => message.Exclude = true);
                    cfg.DeployPublishTopology = true;
                    cfg.UseDelayedRedelivery(message => message.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
                    cfg.UseMessageRetry(message => message.Immediate(5));
                    cfg.Host(rabbitmqConnectionString);
                    cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("catalog", false));
                });
            });
            builder.Services.AddHealthChecks().AddRabbitMQ(
                sp =>
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(rabbitmqConnectionString),
                    AutomaticRecoveryEnabled = true
                };
                return factory.CreateConnectionAsync();
            }, timeout: TimeSpan.FromSeconds(5),
                tags: ["messagebus", "rabbitmq"]);

            // Automatically register services.
            builder.Services.Scan(selector => selector
            .FromAssemblies(
                applicationAssembly,
                dbContextAssembly)
            .AddClasses(publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork<AppDbContext>>();

            ////builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        }

        /// <summary>
        /// Use catalog infrastructure.
        /// </summary>
        /// <param name="app">The app.</param>
        public static void UseCatalogInfrastructure(this WebApplication app)
        {
            // Method intentionally left empty.
        }
    }
}
