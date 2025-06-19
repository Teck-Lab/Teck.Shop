using System.Text.Json.Serialization;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Teck.Shop.SharedKernel.Infrastructure.Logging;
using Teck.Shop.SharedKernel.Infrastructure.Middlewares;
using Teck.Shop.SharedKernel.Infrastructure.Options;

namespace Teck.Shop.SharedKernel.Infrastructure
{
    /// <summary>
    /// The extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Allow all origins.
        /// </summary>
        public const string AllowAllOrigins = "AllowAll";

        /// <summary>
        /// Add the infrastructure.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="appOptions"></param>
        public static void AddBaseInfrastructure(
            this WebApplicationBuilder builder,
            AppOptions appOptions)
        {
            builder.ConfigureSerilog(appOptions.Name);

            // 1. Core services
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDefaultCorrelationId(options =>
            {
                options.RequestHeader = "X-Correlation-ID";
                options.ResponseHeader = "X-Correlation-ID";
                options.IncludeInResponse = true;
                options.AddToLoggingScope = true;
                options.UpdateTraceIdentifier = false;

                options.CorrelationIdGenerator = () => Guid.NewGuid().ToString();
            });

            builder.Services.AddRouting(options => options.LowercaseUrls = true);

            // 2. CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });


            // 3. Forwarded headers
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });


        }

        /// <summary>
        /// Use the infrastructure.
        /// </summary>
        /// <param name="app">The app.</param>
        public static void UseBaseInfrastructure(
            this WebApplication app)
        {
            app.UseCorrelationId();

            // Add global exception handler middleware here
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            // Preserve Order
            app.UseCors(AllowAllOrigins);
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor
                                 | ForwardedHeaders.XForwardedProto
                                 | ForwardedHeaders.XForwardedHost
            });

            app.Use((context, next) =>
            {
                if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var prefix))
                {
                    context.Request.PathBase = new PathString(prefix);
                }

                return next();
            });

            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
