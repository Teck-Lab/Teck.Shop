using Catalog.Application.Features.Products.CreateProduct.V1;
using Catalog.Application.Features.Products.GetProductById.V1;
using Catalog.Application.Features.Products.Response;
using ErrorOr;
using FastEndpoints;
using Keycloak.AuthServices.Authorization;
using Mediator;
using Teck.Shop.SharedKernel.Infrastructure.Endpoints;

namespace Catalog.Api.Endpoints.V1.Products
{
    /// <summary>
    /// The create product endpoint.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CreateProductEndpoint"/> class.
    /// </remarks>
    /// <param name="mediatr">The mediatr.</param>
    public class CreateProductEndpoint(ISender mediatr) : Endpoint<CreateProductRequest, ProductResponse>
    {
        private readonly ISender _mediatr = mediatr;

        /// <summary>
        /// Configures the endpoint for creating a product.
        /// </summary>
        public override void Configure()
        {
            Post("/Products");
            Options(ep => ep.RequireProtectedResource("products", "create")/* .AddEndpointFilter<IdempotentAPIEndpointFilter>() */);
            Version(0);
            Validator<CreateProductValidator>();
        }

        /// <summary>
        /// Handles the HTTP request to create a new product.
        /// </summary>
        /// <param name="req">The request containing product details.</param>
        /// <param name="ct">The cancellation token.</param>
        public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
        {
            CreateProductCommand command = new(req.Name, req.Description, req.ProductSku, req.GTIN, req.IsActive, req.BrandId, req.CategoryIds);
            ErrorOr<ProductResponse> commandResponse = await _mediatr.Send(command, ct);
            await this.SendCreatedAtAsync<GetProductByIdEndpoint, ErrorOr<ProductResponse>>(routeValues: new { commandResponse.Value?.Id }, commandResponse, cancellation: ct);
        }
    }
}
