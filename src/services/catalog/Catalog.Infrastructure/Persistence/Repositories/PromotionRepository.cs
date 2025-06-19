using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.PromotionAggregate;
using Microsoft.AspNetCore.Http;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// The promotion repository.
    /// </summary>
    public sealed class PromotionRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        : GenericRepository<Promotion, Guid>(context, httpContextAccessor), IPromotionRepository
    {
    }
}
