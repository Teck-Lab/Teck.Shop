using Catalog.Application.Contracts.Repositories;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.Mappings;
using Catalog.Domain.Entities.BrandAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Teck.Shop.SharedKernel.Core.Pagination;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// The brand repository.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BrandRepository"/> class.
    /// </remarks>
    /// <param name="context">The context.</param>
    /// <param name="httpContextAccessor">The http context accessor.</param>
    public sealed class BrandRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor) : GenericRepository<Brand, Guid>(context, httpContextAccessor), IBrandRepository
    {
        /// <summary>
        /// The context.
        /// </summary>
        private readonly AppDbContext _context = context;

        /// <summary>
        /// Get paged brands asynchronously.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="size">The size.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<PagedList<Brand>>]]></returns>
        public async Task<PagedList<Brand>> GetPagedBrandsAsync(int page, int size, string? keyword, CancellationToken cancellationToken = default)
        {
            IQueryable<Brand> queryable = _context.Brands.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowered = keyword.ToLowerInvariant();
                queryable = queryable.Where(brand => brand.Name.ToLower().Contains(lowered));
            }

            queryable = queryable.OrderBy(brand => brand.CreatedOn);

            return await queryable.ApplyPagingAsync<Brand>(
                page,
                size,
                cancellationToken);
        }

        /// <summary>
        /// Deletes the brands asynchronously.
        /// </summary>
        /// <param name="Ids">The ids.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public async Task DeleteBrandsAsync(ICollection<Guid> Ids, CancellationToken cancellationToken = default)
        {
            IQueryable<Brand> queryable = _context.Brands.AsQueryable();
            await queryable.Where(existingBrand => Ids.Any(brandInput => existingBrand.Id.Equals(brandInput))).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
