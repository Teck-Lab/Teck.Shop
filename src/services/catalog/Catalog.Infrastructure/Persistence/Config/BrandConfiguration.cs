using Catalog.Domain.Entities.BrandAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Config
{
    /// <summary>
    /// Provides Entity Framework configuration for the <see cref="Brand"/> entity.
    /// </summary>
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        /// <summary>
        /// Configures the Brand entity type.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the Brand entity.</param>
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.OwnsOne(b => b.Website, w =>
            {
                w.Property(x => x.Value)
                 .HasColumnName("Website")
                 .HasMaxLength(2048); // Adjust as needed
            });
        }
    }
}