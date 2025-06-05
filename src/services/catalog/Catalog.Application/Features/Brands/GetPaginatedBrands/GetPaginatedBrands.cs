using Catalog.Application.Contracts.Repositories;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.Mappings;
using Teck.Shop.SharedKernel.Core.CQRS;
using Teck.Shop.SharedKernel.Core.Pagination;

namespace Catalog.Application.Features.Brands.GetPaginatedBrands
{
    /// <summary>
    /// Get paginated brands query.
    /// </summary>
    public sealed record GetPaginatedBrandsQuery(int Page, int Size, string? Keyword) : IQuery<PagedList<BrandResponse>>;

    /// <summary>
    /// Get paginated brands query handler.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GetPaginatedBrandsQueryHandler"/> class.
    /// </remarks>
    /// <param name="brandRepository">The brand repository.</param>
    internal sealed class GetPaginatedBrandsQueryHandler(IBrandRepository brandRepository) : IQueryHandler<GetPaginatedBrandsQuery, PagedList<BrandResponse>>
    {
        /// <summary>
        /// The brand repository.
        /// </summary>
        private readonly IBrandRepository _brandRepository = brandRepository;

        /// <summary>
        /// Handle and return a task of a pagedlist of brandresponses.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<PagedList<BrandResponse>>]]></returns>
        public async ValueTask<PagedList<BrandResponse>> Handle(GetPaginatedBrandsQuery request, CancellationToken cancellationToken)
        {
            var pagedBrands =  await _brandRepository.GetPagedBrandsAsync(request.Page, request.Size, request.Keyword, cancellationToken);

            return BrandMapper.PagedBrandToPagedBrandResponse(pagedBrands);
        }
    }
}
