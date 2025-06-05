using System.Reflection;
using System.Text.Json.Serialization;
using CorrelationId.Abstractions;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Teck.Shop.SharedKernel.Infrastructure.Endpoints
{
    /// <summary>
    /// The extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Add fast endpoints extension.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="applicationAssembly">The application assembly.</param>
        public static IServiceCollection AddFastEndpointsInfrastructure(
            this IServiceCollection services,
            Assembly? applicationAssembly = null)
        {
            services.AddFastEndpoints(ep =>
            {
                if (applicationAssembly is not null)
                {
                    ep.Assemblies = [applicationAssembly];
                }
            }).AddIdempotency();

            services.AddValidatorsFromAssembly(applicationAssembly, filter: filter => filter.ValidatorType.BaseType?.GetGenericTypeDefinition() != typeof(FastEndpoints.Validator<>));

            return services;
        }

        /// <summary>
        /// Use swagger extension.
        /// </summary>
        /// <param name="app">The app.</param>
        public static IApplicationBuilder UseFastEndpointsInfrastructure(this IApplicationBuilder app)
        {
            app.UseOutputCache().UseFastEndpoints(config =>
            {
                config.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
                {
                    var accessor = ctx.RequestServices.GetService<ICorrelationContextAccessor>();
                    var correlationId = accessor?.CorrelationContext?.CorrelationId ?? Guid.NewGuid().ToString();

                    ctx.Response.Headers["X-Correlation-ID"] = correlationId;

                    var problemDetails = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(
                        failures.GroupBy(f => f.PropertyName)
                                .ToDictionary(
                                    keySelector: e => e.Key,
                                    elementSelector: e => e.Select(m => m.ErrorMessage).ToArray()))
                    {
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = statusCode,
                        Instance = ctx.Request.Path
                    };

                    problemDetails.Extensions["traceId"] = ctx.TraceIdentifier;
                    problemDetails.Extensions["correlationId"] = correlationId;

                    return problemDetails;
                };

                config.Serializer.Options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                config.Versioning.Prefix = "v";
                config.Versioning.PrependToRoute = true;
                config.Versioning.DefaultVersion = 1;
            });

            return app;
        }
    }
}
