using Catalog.Application.Contracts.Caching;
using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.BrandAggregate;
using Teck.Shop.SharedKernel.Persistence.Caching;
using ZiggyCreatures.Caching.Fusion;

namespace Catalog.Infrastructure.Caching
{
    /// <summary>
    /// The brand cache.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BrandCache"/> class.
    /// </remarks>
    /// <param name="brandCache"></param>
    /// <param name="brandRepository">The brand repository.</param>
    public class BrandCache(IFusionCache brandCache, IBrandRepository brandRepository) : GenericCacheService<Brand, Guid>(brandCache, brandRepository), IBrandCache
    {
    }
}
