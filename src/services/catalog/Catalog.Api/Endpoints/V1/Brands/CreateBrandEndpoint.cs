using Catalog.Application.Features.Brands.CreateBrand.V1;
using Catalog.Application.Features.Brands.Dtos;
using ErrorOr;
using FastEndpoints;
using Keycloak.AuthServices.Authorization;
using Mediator;
using Teck.Shop.SharedKernel.Infrastructure.Endpoints;

namespace Catalog.Api.Endpoints.V1.Brands
{
    /// <summary>
    /// The create brand endpoint.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CreateBrandEndpoint"/> class.
    /// </remarks>
    /// <param name="mediatr">The mediatr.</param>
    public class CreateBrandEndpoint(ISender mediatr) : Endpoint<CreateBrandRequest, BrandResponse>
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
            Post("/Brands");
            Options(ep => ep.RequireProtectedResource("brand", "create")/*.AddEndpointFilter<IdempotentAPIEndpointFilter>()*/);
            Validator<CreateBrandValidator>();
            Version(1, 3);
            
        }

        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public override async Task HandleAsync(CreateBrandRequest req, CancellationToken ct)
        {
            CreateBrandCommand command = new(req.Name, req.Description, req.Website);
            ErrorOr<BrandResponse> commandResponse = await _mediatr.Send(command, ct);
            await this.SendCreatedAtAsync<GetBrandEndpoint, ErrorOr<BrandResponse>>(routeValues: new { commandResponse.Value?.Id }, commandResponse, cancellation: ct);
        }
    }
}
