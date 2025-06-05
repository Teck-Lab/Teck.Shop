namespace Catalog.Application.Features.Brands.DeleteBrands
{
    /// <summary>
    /// The delete brands request.
    /// </summary>
    public sealed record DeleteBrandsRequest
    {
        /// <summary>
        /// Gets or sets the ids.
        /// </summary>
        public IReadOnlyCollection<Guid> Ids { get; set; } = [];
    }
}
