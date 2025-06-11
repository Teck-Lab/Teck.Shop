using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Application.Features.Products.GetProductById.V1;
using Catalog.Application.Features.Products.Response;
using Catalog.Application.Contracts.Caching;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.ProductAggregate.Errors;
using ErrorOr;
using NSubstitute;
using Xunit;
using Teck.Shop.SharedKernel.Core.CQRS;

namespace Catalog.UnitTests.Application.Products
{
    public class GetProductByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_Should_ReturnProductResponse_WhenProductExists()
        {
            var cache = Substitute.For<IProductCache>();
            var handler = new GetProductByIdQueryHandler(cache);
            var product = new Product();
            var query = new GetProductByIdQuery(Guid.NewGuid());
            cache.GetOrSetByIdAsync(query.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(product);
            var handlerInterface = (IQueryHandler<GetProductByIdQuery, ErrorOr<ProductResponse>>)handler;
            var result = await handlerInterface.Handle(query, CancellationToken.None);
            Assert.False(result.IsError);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_WhenProductDoesNotExist()
        {
            var cache = Substitute.For<IProductCache>();
            var handler = new GetProductByIdQueryHandler(cache);
            var query = new GetProductByIdQuery(Guid.NewGuid());
            cache.GetOrSetByIdAsync(query.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns((Product)null);
            var handlerInterface = (IQueryHandler<GetProductByIdQuery, ErrorOr<ProductResponse>>)handler;
            var result = await handlerInterface.Handle(query, CancellationToken.None);
            Assert.True(result.IsError);
            Assert.Equal(ProductErrors.NotFound, result.FirstError);
        }
    }
}
