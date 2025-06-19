using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.SupplierAggregate;
using Microsoft.AspNetCore.Http;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// The supplier repository.
    /// </summary>
    public sealed class SupplierRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        : GenericRepository<Supplier, Guid>(context, httpContextAccessor), ISupplierRepository
    {
    }
}
