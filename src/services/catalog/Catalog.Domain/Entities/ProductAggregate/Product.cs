using System.Text.RegularExpressions;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductAggregate.Errors;
using Catalog.Domain.Entities.ProductAggregate.Events;
using Catalog.Domain.Entities.PromotionAggregate;
using ErrorOr;
using Teck.Shop.SharedKernel.Core.Domain;

namespace Catalog.Domain.Entities.ProductAggregate
{
    /// <summary>
    /// The product.
    /// </summary>
    public class Product : BaseEntity, IAggregateRoot
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; } = default!;

        /// <summary>
        /// Gets the details.
        /// </summary>
        public string? Description { get; private set; } = default!;

        /// <summary>
        /// Gets the slug.
        /// </summary>
        public string Slug { get; private set; } = default!;

        /// <summary>
        /// Gets a value indicating whether active.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets the product SKU.
        /// </summary>
        public string ProductSKU { get; private set; } = default!;

        /// <summary>
        /// Gets the GTIN.
        /// </summary>
        public string? GTIN { get; private set; } = default!;

        /// <summary>
        /// Gets the Brand id.
        /// </summary>
        public Guid? BrandId { get; private set; }

        /// <summary>
        /// Gets the brand.
        /// </summary>
        public Brand? Brand { get; private set; }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        public ICollection<Category> Categories { get; private set; } = [];

        /// <summary>
        /// Gets the product prices.
        /// </summary>
        public ICollection<ProductPrice> ProductPrices { get; private set; } = [];

        /// <summary>
        /// Gets the promotions.
        /// </summary>
        public ICollection<Promotion> Promotions { get; private set; } = [];

        /// <summary>
        /// Update a brand.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ErrorOr<Updated> Update(string? name)
        {
            var errors = new List<Error>();

            if (name is not null)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add(ProductErrors.EmptyName);
                }
                else if (!Name.Equals(name, StringComparison.Ordinal))
                {
                    Name = name;
                    Slug = GetProductSlug(name);
                }
            }

            if (errors.Any())
            {
                return errors;
            }

            return Result.Updated;
        }

        /// <summary>
        /// Create a brand.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="sku"></param>
        /// <param name="gtin"></param>
        /// <param name="categories"></param>
        /// <param name="isActive"></param>
        /// <param name="brand"></param>
        public static ErrorOr<Product> Create(
            string name,
            string? description,
            string? sku,
            string? gtin,
            List<Category> categories,
            bool isActive,
            Brand brand)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(ProductErrors.EmptyName);
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                errors.Add(ProductErrors.EmptyDescription);
            }

            if (string.IsNullOrWhiteSpace(sku))
            {
                errors.Add(ProductErrors.EmptySKU);
            }

            if (string.IsNullOrWhiteSpace(gtin))
            {
                errors.Add(ProductErrors.EmptyGTIN);
            }

            if (categories == null || !categories.Any())
            {
                errors.Add(ProductErrors.EmptyCategories);
            }

            if (brand == null)
            {
                errors.Add(ProductErrors.NullBrand);
            }

            if (errors.Any())
            {
                return errors;
            }

            var product = new Product
            {
                Name = name,
                Description = description,
                ProductSKU = sku,
                GTIN = gtin,
                Categories = categories,
                IsActive = isActive,
                Brand = brand,
                BrandId = brand.Id,
                Slug = name.ToLower().Replace(" ", "-")
            };

            product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, product.Name));

            return product;
        }

        /// <summary>
        /// Get product slug.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        private static string GetProductSlug(string name)
        {
            name = name.Trim();
            name = name.ToLower();
            name = Regex.Replace(name, "[^a-z0-9]+", "-");
            name = Regex.Replace(name, "--+", "-");
            name = name.Trim('-');
            return name;
        }
    }
}
