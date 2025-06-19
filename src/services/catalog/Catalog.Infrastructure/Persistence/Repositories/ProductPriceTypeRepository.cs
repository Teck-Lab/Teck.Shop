using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Microsoft.AspNetCore.Http;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// The product price type repository.
    /// </summary>
    public sealed class ProductPriceTypeRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        : GenericRepository<ProductPriceType, Guid>(context, httpContextAccessor), IProductPriceTypeRepository
    {
    }
}
