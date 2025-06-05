using Catalog.Domain.Entities.BrandAggregate;
using Teck.Shop.SharedKernel.Core.Caching;

namespace Catalog.Application.Contracts.Caching
{
    /// <summary>
    /// Branc cache interface.
    /// </summary>
    public interface IBrandCache : IGenericCacheService<Brand, Guid>
    {
    }
}
