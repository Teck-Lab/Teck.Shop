using Catalog.Application.Contracts.Caching;
using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.ProductAggregate;
using Teck.Shop.SharedKernel.Persistence.Caching;
using ZiggyCreatures.Caching.Fusion;

namespace Catalog.Infrastructure.Caching
{
    /// <summary>
    /// The product cache.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductCache"/> class.
    /// </remarks>
    /// <param name="brandCache">The brand cache.</param>
    /// <param name="brandRepository">The brand repository.</param>
    public class ProductCache(IFusionCache brandCache, IProductRepository brandRepository) : GenericCacheService<Product, Guid>(brandCache, brandRepository), IProductCache
    {
    }
}
