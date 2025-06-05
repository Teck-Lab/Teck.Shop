using Catalog.Application.Features.Products.Response;
using Catalog.Domain.Entities.ProductAggregate;
using Riok.Mapperly.Abstractions;

namespace Catalog.Application.Features.Products
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    internal static partial class ProductMappings
    {
        internal static partial ProductResponse ProductToProductResponse(this Product product);
    }
}
