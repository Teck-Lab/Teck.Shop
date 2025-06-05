using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.GetPaginatedBrands;
using FastEndpoints;
using Keycloak.AuthServices.Authorization;
using Mediator;
using Teck.Shop.SharedKernel.Core.Pagination;

namespace Catalog.Api.Endpoints.V1.Brands
{
    /// <summary>
    /// The get paginated brands endpoint.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GetPaginatedBrandsEndpoint"/> class.
    /// </remarks>
    /// <param name="mediatr">The mediatr.</param>
    public class GetPaginatedBrandsEndpoint(ISender mediatr) : Endpoint<GetPaginatedBrandsRequest, PagedList<BrandResponse>>
    {
        /// <summary>
        /// The mediatr.
        /// </summary>
        private readonly ISender _mediatr = mediatr;

        /// <summary>
        /// Configure the endpoint.
        /// </summary>
        public override void Configure()
        {
            Get("/Brands");
            Options(ep => ep.RequireProtectedResource("brand", "list"));
            Version(1);
            Validator<GetPaginatedBrandsValidator>();
        }

        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public override async Task HandleAsync(GetPaginatedBrandsRequest req, CancellationToken ct)
        {
            GetPaginatedBrandsQuery query = new(req.Page, req.Size, req.Keyword);
            PagedList<BrandResponse> queryResponse = await _mediatr.Send(query, ct);
            await SendAsync(queryResponse, cancellation: ct);
        }
    }
}
