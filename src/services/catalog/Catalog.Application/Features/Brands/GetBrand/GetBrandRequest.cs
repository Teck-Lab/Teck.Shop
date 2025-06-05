namespace Catalog.Application.Features.Brands.GetBrand
{
    /// <summary>
    /// The get brand request.
    /// </summary>
    public sealed record GetBrandRequest
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }
    }
}
