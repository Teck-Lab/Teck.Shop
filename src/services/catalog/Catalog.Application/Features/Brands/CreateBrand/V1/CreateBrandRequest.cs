namespace Catalog.Application.Features.Brands.CreateBrand.V1
{
    /// <summary>
    /// The create brand request.
    /// </summary>
    public sealed record CreateBrandRequest
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the website.
        /// </summary>
        public string? Website { get; set; }
    }
}
