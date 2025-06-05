using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Teck.Shop.SharedKernel.Core.Domain;

namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore
{
    /// <summary>
    /// The base db context.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
    /// </remarks>
    /// <param name="options">The options.</param>
    public abstract class BaseDbContext(DbContextOptions options) : DbContext(options), IBaseDbContext
    {
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
