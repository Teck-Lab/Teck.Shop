using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.CategoryAggregate;
using Microsoft.AspNetCore.Http;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// The category repository.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
    /// </remarks>
    /// <param name="context"></param>
    /// <param name="httpContextAccessor"></param>
    public sealed class CategoryRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor) : GenericRepository<Category, Guid>(context, httpContextAccessor), ICategoryRepository
    {
    }
}
