using Catalog.Domain.Entities.ProductAggregate;
using Teck.Shop.SharedKernel.Core.Caching;

namespace Catalog.Application.Contracts.Caching
{
    /// <summary>
    /// Product cache interface.
    /// </summary>
    public interface IProductCache : IGenericCacheService<Product, Guid>
    {
    }
}
