using System.Reflection;
using Catalog.Application;
using Catalog.Application.Features.Brands.GetBrand;
using Catalog.Application.Features.Brands.GetPaginatedBrands;
using Teck.Shop.SharedKernel.Infrastructure.Behaviors;

namespace Catalog.Api.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Mediator infrastructure in a <see cref="WebApplicationBuilder"/>.
    /// </summary>
    public static class MediatorExtension
    {
        /// <summary>
        /// Registers Mediator infrastructure, including handler scanning and pipeline behaviors.
        /// Configures Mediator to scan the specified application assembly for handlers and behaviors,
        /// and registers custom pipeline behaviors in the defined order.
        /// </summary>
        /// <param name="builder">The <see cref="WebApplicationBuilder"/> used to configure services.</param>
        /// <param name="applicationAssembly">The assembly containing the application-specific Mediator handlers and behaviors.</param>
        /// <returns>The same <see cref="WebApplicationBuilder"/> instance for chaining.</returns>
        public static WebApplicationBuilder AddMediatorInfrastructure(
            this WebApplicationBuilder builder,
            Assembly applicationAssembly)
        {
            builder.Services.AddMediator((Mediator.MediatorOptions options) =>
            {
                // Specify the assembly to scan for Mediator handlers and pipeline behaviors.
                options.Assemblies = [typeof(ICatalogApplication)];
                options.ServiceLifetime = ServiceLifetime.Scoped;
                // Configure the request pipeline by registering behaviors in the desired order.
                // Behaviors are executed in listed order, wrapping around the core handler.
                options.PipelineBehaviors =
                [
                typeof(LoggingBehavior<,>),         // Logs request start, end, and duration.
                typeof(TransactionalBehavior<,>),   // Wraps command handling in a DB transaction.
            ];
            });

            return builder;
        }
    }
}
