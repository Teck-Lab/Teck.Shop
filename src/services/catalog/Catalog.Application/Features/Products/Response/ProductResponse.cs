using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Categories.Response;
using Catalog.Application.Features.ProductPrices.Response;
using Catalog.Application.Features.Promotions.Response;

namespace Catalog.Application.Features.Products.Response
{
    /// <summary>
    /// The product response.
    /// </summary>
    [Serializable]
    public record ProductResponse
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the product SKU.
        /// </summary>
        public string ProductSKU { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the GTIN.
        /// </summary>
        public string? GTIN { get; set; }

        /// <summary>
        /// Gets or sets the brand id.
        /// </summary>
        public Guid? BrandId { get; set; }

        /// <summary>
        /// Gets or sets the brand.
        /// </summary>
        public BrandResponse? Brand { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        public ICollection<CategoryResponse> Categories { get; set; } = [];

        /// <summary>
        /// Gets or sets the product prices.
        /// </summary>
        public ICollection<ProductPriceResponse> ProductPrices { get; set; } = [];

        /// <summary>
        /// Gets or sets the promotions.
        /// </summary>
        public ICollection<PromotionResponse> Promotions { get; set; } = [];
    }
}
