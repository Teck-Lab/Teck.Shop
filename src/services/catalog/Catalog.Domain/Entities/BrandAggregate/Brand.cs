using Catalog.Domain.Entities.BrandAggregate.Errors;
using Catalog.Domain.Entities.BrandAggregate.Events;
using Catalog.Domain.Entities.ProductAggregate;
using ErrorOr;
using Teck.Shop.SharedKernel.Core.Domain;

namespace Catalog.Domain.Entities.BrandAggregate
{
    /// <summary>
    /// The brand.
    /// </summary>
    public class Brand : BaseEntity, IAggregateRoot
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; } = default!;

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string? Description { get; private set; } = null!;

        /// <summary>
        /// Gets the website.
        /// </summary>
        public string? Website { get; private set; } = null!;

        /// <summary>
        /// Gets the products.
        /// </summary>
        public ICollection<Product> Products { get; private set; } = [];

        /// <summary>
        /// Update a brand.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="website"></param>
        /// <returns></returns>
        public ErrorOr<Updated> Update(string? name, string? description, string? website)
        {
            var errors = new List<Error>();

            if (name is not null)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add(BrandErrors.EmptyName);
                }
                else if (!string.Equals(Name, name, StringComparison.Ordinal))
                {
                    Name = name;
                }
            }

            if (description is not null)
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    errors.Add(BrandErrors.EmptyDescription);
                }
                else if (!string.Equals(Description, description, StringComparison.Ordinal))
                {
                    Description = description;
                }
            }

            if (website is not null)
            {
                if (string.IsNullOrWhiteSpace(website))
                {
                    errors.Add(BrandErrors.EmptyWebsite);
                }
                else if (!Uri.IsWellFormedUriString(website, UriKind.Absolute)
                    || !(website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || website.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add(BrandErrors.InvalidWebsite);
                }
                else if (!string.Equals(Website, website, StringComparison.Ordinal))
                {
                    Website = website;
                }
            }
            // If website is null, do not change the property (preserve existing value)

            if (errors.Any())
            {
                return errors;
            }

            return Result.Updated;
        }

        /// <summary>
        /// Create a brand.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="website"></param>
        /// <returns></returns>
        public static ErrorOr<Brand> Create(string name, string? description, string? website)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(BrandErrors.EmptyName);
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                errors.Add(BrandErrors.EmptyDescription);
            }

            if (website is not null)
            {
                if (string.IsNullOrWhiteSpace(website))
                {
                    errors.Add(BrandErrors.EmptyWebsite);
                }
                else if (!Uri.IsWellFormedUriString(website, UriKind.Absolute)
                    || !(website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || website.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add(BrandErrors.InvalidWebsite);
                }
            }

            if (errors.Any())
            {
                return errors;
            }

            var brand = new Brand
            {
                Name = name,
                Description = description,
                Website = website
            };

            var @event = new BrandCreatedDomainEvent(brand.Id, brand.Name);
            brand.AddDomainEvent(@event);

            return brand;
        }
    }
}
