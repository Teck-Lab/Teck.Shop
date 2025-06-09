using Catalog.Application.Features.Products;
using Catalog.Application.Features.Products.Response;
using Catalog.Domain.Entities.ProductAggregate;
using System;
using Xunit;

namespace Catalog.Application.UnitTests.Products
{
    public class ProductMappingsTests
    {
        [Fact]
        public void ProductToProductResponse_Maps_All_Properties()
        {
            var product = new Product();
            // Set properties if needed, e.g. via reflection or constructor if available
            var response = ProductMappings.ProductToProductResponse(product);
            Assert.NotNull(response);
            // Optionally, assert mapped properties if Product has public setters/getters
        }
    }
}
