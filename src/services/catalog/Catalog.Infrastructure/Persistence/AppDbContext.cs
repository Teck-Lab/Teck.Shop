using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Catalog.Domain.Entities.PromotionAggregate;
using Catalog.Domain.Entities.SupplierAggregate;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;

namespace Catalog.Infrastructure.Persistence
{
    /// <summary>
    /// The app db context.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </remarks>
    /// <param name="options">The options.</param>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : BaseDbContext(options)
    {
        /// <summary>
        /// On model creating.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }

        /// <summary>
        /// Gets or sets the brands.
        /// </summary>
        public DbSet<Brand> Brands { get; set; }

        /// <summary>
        /// Gets or sets the products.
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Gets or sets the product prices.
        /// </summary>
        public DbSet<ProductPrice> ProductPrices { get; set; }

        /// <summary>
        /// Gets or sets the product price types.
        /// </summary>
        public DbSet<ProductPriceType> ProductPriceTypes { get; set; }

        /// <summary>
        /// Gets or sets the promotions.
        /// </summary>
        public DbSet<Promotion> Promotions { get; set; }

        /// <summary>
        /// Gets or sets the suppliers.
        /// </summary>
        public DbSet<Supplier> Suppliers { get; set; }
    }
}
