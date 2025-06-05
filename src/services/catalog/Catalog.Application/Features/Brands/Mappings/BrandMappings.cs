using Catalog.Application.Features.Brands.Dtos;
using Catalog.Domain.Entities.BrandAggregate;
using Riok.Mapperly.Abstractions;
using Teck.Shop.SharedKernel.Core.Pagination;

namespace Catalog.Application.Features.Brands.Mappings
{
    /// <summary>
    /// The brand mappings.
    /// </summary>
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
    public static partial class BrandMapper
    {
        /// <summary>
        /// Brand converts to brand response.
        /// </summary>
        /// <param name="brand">The brand.</param>
        /// <returns>A BrandResponse</returns>
        public static partial BrandResponse BrandToBrandResponse(Brand brand);
        /// <summary>
        /// Maps a paged list of Brand entities to a paged list of BrandResponse DTOs.
        /// </summary>
        /// <param name="brands">The paginated brand list.</param>
        /// <returns>A paged list of BrandResponse DTOs.</returns>
        public static partial PagedList<BrandResponse> PagedBrandToPagedBrandResponse(PagedList<Brand> brands);
    }

}
