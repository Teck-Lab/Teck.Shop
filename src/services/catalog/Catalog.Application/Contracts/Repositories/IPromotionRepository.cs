using Catalog.Domain.Entities.PromotionAggregate;
using Teck.Shop.SharedKernel.Core.Database;

namespace Catalog.Application.Contracts.Repositories
{
    /// <summary>
    /// Promotion repository interface.
    /// </summary>
    public interface IPromotionRepository : IGenericRepository<Promotion, Guid>
    {
    }
}
