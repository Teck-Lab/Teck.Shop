using Catalog.Domain.Entities.CategoryAggregate;
using Teck.Shop.SharedKernel.Core.Database;

namespace Catalog.Application.Contracts.Repositories
{
    /// <summary>
    /// Category repository interface.
    /// </summary>
    public interface ICategoryRepository : IGenericRepository<Category, Guid>
    {
    }
}
