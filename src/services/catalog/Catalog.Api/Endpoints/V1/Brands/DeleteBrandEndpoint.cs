using Catalog.Application.Features.Brands.DeleteBrand;
using ErrorOr;
using FastEndpoints;
using Keycloak.AuthServices.Authorization;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Teck.Shop.SharedKernel.Infrastructure.Endpoints;

namespace Catalog.Api.Endpoints.V1.Brands
{
    /// <summary>
    /// The delete brand endpoint.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DeleteBrandEndpoint"/> class.
    /// </remarks>
    /// <param name="mediatr">The mediatr.</param>
    public class DeleteBrandEndpoint(ISender mediatr) : Endpoint<DeleteBrandRequest, NoContent>
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
            Delete("/Brands/{Id}");
            Options(ep => ep.RequireProtectedResource("brand", "delete"));
            Version(1);
            Validator<DeleteBrandValidator>();
        }

        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public override async Task HandleAsync(DeleteBrandRequest req, CancellationToken ct)
        {
            DeleteBrandCommand command = new(req.Id);
            ErrorOr<Deleted> commandResponse = await _mediatr.Send(command, ct);
            await this.SendNoContentResponseAsync(commandResponse, cancellation: ct);
        }
    }
}
