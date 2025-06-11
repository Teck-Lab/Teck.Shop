using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Teck.Shop.SharedKernel.Core.Database;

namespace Catalog.Application.Contracts.Repositories
{
    /// <summary>
    /// Product price type repository interface.
    /// </summary>
    public interface IProductPriceTypeRepository : IGenericRepository<ProductPriceType, Guid>
    {
    }
}
