using System.Text.Json;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Teck.Shop.SharedKernel.Infrastructure.Auth
{
    /// <summary>
    /// Provides authentication and authorization setup using Keycloak.
    /// Can be used in both web APIs and proxy gateways (e.g., YARP).
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Configures Keycloak authentication and authorization.
        /// This extension does not rely on any web framework (e.g., FastEndpoints), making it safe for YARP.
        /// </summary>
        /// <param name="services">The application's service collection.</param>
        /// <param name="config">The application configuration (used to bind Keycloak options).</param>
        /// <param name="env">The hosting environment (used to determine HTTPS requirements).</param>
        /// <param name="keycloakOptions">Bound Keycloak options, e.g., realm, URL, resource name.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddKeycloak(
            this IServiceCollection services,
            IConfiguration config,
            IHostEnvironment env,
            KeycloakAuthenticationOptions keycloakOptions)
        {
            bool isProduction = env.IsProduction();

            // Configure JWT Bearer authentication from Keycloak
            services.AddKeycloakWebApiAuthentication(config, options =>
            {
                options.IncludeErrorDetails = true;
                options.Authority = keycloakOptions.KeycloakUrlRealm;
                options.Audience = keycloakOptions.Resource;
                options.RequireHttpsMetadata = isProduction;
                options.SaveToken = true;

                // Customize token validation
                options.TokenValidationParameters = new()
                {
                    RequireAudience = true,
                    ValidateAudience = true,
                };

                // Custom response for unauthorized and forbidden access
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerEvents");

                        var traceId = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
                        var correlationId = context.HttpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                                           ?? context.HttpContext.TraceIdentifier;
                        var userId = context.HttpContext.User?.Identity?.Name ?? "anonymous";

                        var details = new[]
                        {
            new
            {
                name = "authorization",
                reason = context.ErrorDescription ?? "Authentication is required to access this resource."
            }
                        };

                        var problem = new ProblemDetails
                        {
                            Status = 401,
                            Title = "Unauthorized",
                            Type = "https://tools.ietf.org/html/rfc7235",
                            Detail = "Authentication failed or was missing.",
                            Extensions =
                            {
                ["traceId"] = traceId,
                ["correlationId"] = correlationId,
                ["details"] = details
                            }
                        };

                        logger.LogWarning("401 Unauthorized - Path: {Path}, User: {User}, TraceId: {TraceId}, Error: {Error}, Description: {Description}",
                            context.HttpContext.Request.Path,
                            userId,
                            traceId,
                            context.Error ?? "N/A",
                            context.ErrorDescription ?? "N/A");

                        context.Response.StatusCode = problem.Status ?? 401;
                        context.Response.ContentType = "application/problem+json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    },

                    OnForbidden = async context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerEvents");

                        var traceId = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
                        var correlationId = context.HttpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                                           ?? context.HttpContext.TraceIdentifier;
                        var userId = context.HttpContext.User?.Identity?.Name ?? "anonymous";

                        var details = new[]
                        {
            new
            {
                name = "authorization",
                reason = "You do not have permission to access this resource."
            }
                        };

                        var problem = new ProblemDetails
                        {
                            Status = 403,
                            Title = "Forbidden",
                            Type = "https://tools.ietf.org/html/rfc7235",
                            Detail = "Access denied due to insufficient permissions.",
                            Extensions =
                            {
                ["traceId"] = traceId,
                ["correlationId"] = correlationId,
                ["details"] = details
                            }
                        };

                        logger.LogWarning("403 Forbidden - Path: {Path}, User: {User}, TraceId: {TraceId}",
                            context.HttpContext.Request.Path,
                            userId,
                            traceId);

                        context.Response.StatusCode = problem.Status ?? 403;
                        context.Response.ContentType = "application/problem+json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    }
                };
            });

            // Register authorization using Keycloak's authorization services
            services.AddAuthorization()
                    .AddKeycloakAuthorization()
                    .AddAuthorizationServer(config);

            // Add health check for Keycloak's OpenID Connect server
            services.AddHealthChecks().AddOpenIdConnectServer(
                new Uri($"{keycloakOptions.AuthServerUrl}/realms/{keycloakOptions.Realm}/"),
                tags: ["openId", "identity", "keycloak"],
                failureStatus: HealthStatus.Degraded);

            return services;
        }
    }
}