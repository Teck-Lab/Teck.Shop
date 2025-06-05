using Catalog.Application.Features.Products.Response;
using ErrorOr;
using FastEndpoints;
using Keycloak.AuthServices.Authorization;
using Mediator;
using Teck.Shop.SharedKernel.Infrastructure.Endpoints;

namespace Catalog.Application.Features.Products.GetProductById.V1
{
    /// <summary>
    /// The get product by id endpoint.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GetProductByIdEndpoint"/> class.
    /// </remarks>
    /// <param name="mediatr">The mediatr.</param>
    public class GetProductByIdEndpoint(ISender mediatr) : Endpoint<GetProductByIdRequest, ProductResponse>
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
            Get("/Products/{Id}");
            AllowAnonymous();
            Options(ep => ep.RequireProtectedResource("products", "read"));
            Version(0);
            Validator<GetProductByIdValidator>();
        }

        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
        {
            GetProductByIdQuery query = new(req.ProductId);
            ErrorOr<ProductResponse> queryResponse = await _mediatr.Send(query, ct);
            await this.SendAsync(queryResponse, ct);
        }
    }
}
