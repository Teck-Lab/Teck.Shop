using Catalog.Domain.Entities.SupplierAggregate;
using Teck.Shop.SharedKernel.Core.Database;

namespace Catalog.Application.Contracts.Repositories
{
    /// <summary>
    /// Supplier repository interface.
    /// </summary>
    public interface ISupplierRepository : IGenericRepository<Supplier, Guid>
    {
    }
}
