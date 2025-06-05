using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.GetBrand;
using ErrorOr;
using FastEndpoints;
using Keycloak.AuthServices.Authorization;
using Mediator;
using Teck.Shop.SharedKernel.Infrastructure.Endpoints;

namespace Catalog.Api.Endpoints.V1.Brands
{
    /// <summary>
    /// The get brand endpoint.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GetBrandEndpoint"/> class.
    /// </remarks>
    /// <param name="mediatr">The mediatr.</param>
    public class GetBrandEndpoint(ISender mediatr) : Endpoint<GetBrandRequest, BrandResponse>
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
            Get("/Brands/{Id}");
            AllowAnonymous();
            Options(ep => ep.RequireProtectedResource("brand", "read"));
            Version(1);
            Validator<GetBrandValidator>();
        }

        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public override async Task HandleAsync(GetBrandRequest req, CancellationToken ct)
        {
            GetBrandQuery query = new(req.Id);
            ErrorOr<BrandResponse> queryResponse = await _mediatr.Send(query, ct);
            await this.SendAsync(queryResponse, ct);
        }
    }
}
