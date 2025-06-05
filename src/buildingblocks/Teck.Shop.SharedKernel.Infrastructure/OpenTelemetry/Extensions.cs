using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Teck.Shop.SharedKernel.Infrastructure.OpenTelemetry
{
    internal static class Extensions
    {
        internal static void AddOpenTelemetryExtension(this WebApplicationBuilder builder, string name)
        {
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(name))
                .WithTracing(tracing =>
                {
                    tracing.AddFusionCacheInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddAspNetCoreInstrumentation();
                })
                .WithMetrics(metrics =>
                {
                    metrics.AddFusionCacheInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();
                });
            builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter());
        }
    }
}
