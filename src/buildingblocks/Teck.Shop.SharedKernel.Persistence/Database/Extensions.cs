using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore.Interceptors;

namespace Teck.Shop.SharedKernel.Persistence.Database
{
    /// <summary>
    /// The extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Add db context.
        /// </summary>
        /// <typeparam name="TContext"/>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly"></param>
        /// <param name="connectionString"></param>
        public static void AddCustomDbContext<TContext>(this WebApplicationBuilder builder, Assembly assembly, string connectionString)
            where TContext : BaseDbContext
        {
            builder.Services.AddSingleton<SoftDeleteInterceptor>();
            builder.Services.AddSingleton<AuditingInterceptor>();
            builder.Services.AddSingleton<DomainEventInterceptor>();

            builder.Services.AddDbContext<TContext>((sp, options) =>
            {
                options.UseNpgsql(connectionString, migration => migration.MigrationsAssembly(assembly.FullName));
                options.AddInterceptors(
                    sp.GetRequiredService<SoftDeleteInterceptor>(),
                    sp.GetRequiredService<AuditingInterceptor>(),
                    sp.GetRequiredService<DomainEventInterceptor>());
            });

            builder.EnrichNpgsqlDbContext<TContext>();

            builder.Services.AddHealthChecks().AddNpgSql(
                connectionString: connectionString,
                tags: ["db", "sql", "postgres"]);

            builder.Services.AddScoped(typeof(TContext));
            builder.Services.AddScoped<IBaseDbContext>(sp => sp.GetRequiredService<TContext>());
            builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        }
    }
}
