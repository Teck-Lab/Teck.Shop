using Teck.Shop.SharedKernel.Core.Pagination;

namespace Catalog.Application.Features.Brands.GetPaginatedBrands
{
    /// <summary>
    /// The get paginated brands request.
    /// </summary>
    public class GetPaginatedBrandsRequest : PaginationParameters
    {
        /// <summary>
        /// Gets or sets the keyword.
        /// </summary>
        public string? Keyword { get; set; }
    }
}
