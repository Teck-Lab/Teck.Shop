
using Catalog.Domain.Entities.ProductAggregate;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Core.Pagination;

namespace Catalog.Application.Contracts.Repositories
{
    /// <summary>
    /// Brand repository interface.
    /// </summary>
    public interface IProductRepository : IGenericRepository<Product, Guid>
    {
        /// <summary>
        /// Get paged brands async.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="keyword"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<PagedList<Product>> GetPagedProductsAsync(int page, int size, string? keyword, CancellationToken cancellationToken = default);
    }
}
