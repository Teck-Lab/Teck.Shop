using Catalog.Application.Contracts.Caching;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.Mappings;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.BrandAggregate.Errors;
using ErrorOr;
using Teck.Shop.SharedKernel.Core.CQRS;

namespace Catalog.Application.Features.Brands.GetBrand
{
    /// <summary>
    /// Get Brand query.
    /// </summary>
    public sealed record GetBrandQuery(Guid Id) : IQuery<ErrorOr<BrandResponse>>;

    /// <summary>
    /// Get brand query handler.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GetBrandQueryHandler"/> class.
    /// </remarks>
    /// <param name="cache">The cache.</param>
    internal sealed class GetBrandQueryHandler(IBrandCache cache) : IQueryHandler<GetBrandQuery, ErrorOr<BrandResponse>>
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IBrandCache _cache = cache;

        /// <summary>
        /// Handle and return a task of type erroror.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<ErrorOr<BrandResponse>>]]></returns>
        public async ValueTask<ErrorOr<BrandResponse>> Handle(GetBrandQuery request, CancellationToken cancellationToken)
        {
            Brand? brand = await _cache.GetOrSetByIdAsync(request.Id, cancellationToken: cancellationToken);

            return brand == null ? (ErrorOr<BrandResponse>)BrandErrors.NotFound : (ErrorOr<BrandResponse>)BrandMapper.BrandToBrandResponse(brand);
        }
    }
}
