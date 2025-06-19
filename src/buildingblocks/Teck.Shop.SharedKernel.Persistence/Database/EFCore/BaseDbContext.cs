using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Teck.Shop.SharedKernel.Core.Domain;
using Teck.Shop.SharedKernel.Core.Events;
using MassTransit;
using System.Threading;
using System.Threading.Tasks;

namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore
{
    /// <summary>
    /// The base db context.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
    /// </remarks>
    public abstract class BaseDbContext : DbContext, IBaseDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbContext"/> class with the specified options, event dispatcher, and publish endpoint.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        protected BaseDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// On model creating.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AppendGlobalQueryFilter<ISoftDeletable>(entity => !entity.IsDeleted);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// On configuring.
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseExceptionProcessor();
        }
    }
}
