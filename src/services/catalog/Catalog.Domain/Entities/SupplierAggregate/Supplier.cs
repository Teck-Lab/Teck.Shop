using Catalog.Domain.Entities.SupplierAggregate.Errors;
using ErrorOr;
using Teck.Shop.SharedKernel.Core.Domain;

namespace Catalog.Domain.Entities.SupplierAggregate
{
    /// <summary>
    /// The product.
    /// </summary>
    public class Supplier : BaseEntity, IAggregateRoot
    {
        /// <summary>
        /// Gets the name of the supplier.
        /// </summary>
        public string Name { get; private set; } = default!;

        /// <summary>
        /// Gets the description of the supplier.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Gets the website of the supplier.
        /// </summary>
        public string? Website { get; private set; }

        /// <summary>
        /// Update a brand.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="website"></param>
        /// <returns></returns>
        public ErrorOr<Updated> Update(
            string? name,
            string? description,
            string? website)
        {
            var errors = new List<Error>();

            if (name is not null)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add(SupplierErrors.EmptyName);
                }
                else if (Name?.Equals(name, StringComparison.Ordinal) is not true)
                {
                    Name = name;
                }
            }

            if (Description?.Equals(description, StringComparison.Ordinal) is not true)
            {
                Description = description;
            }

            if (website is not null)
            {
                if (string.IsNullOrWhiteSpace(website))
                {
                    errors.Add(SupplierErrors.EmptyWebsite);
                }
                else if (!Uri.IsWellFormedUriString(website, UriKind.Absolute)
                    || !(website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || website.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add(SupplierErrors.InvalidWebsite);
                }
                else if (Website?.Equals(website, StringComparison.Ordinal) is not true)
                {
                    Website = website;
                }
            }

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
        public static ErrorOr<Supplier> Create(
            string name, string? description, string? website)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(SupplierErrors.EmptyName);
            }

            if (string.IsNullOrWhiteSpace(website))
            {
                errors.Add(SupplierErrors.EmptyWebsite);
            }
            else if (!Uri.IsWellFormedUriString(website, UriKind.Absolute)
                || !(website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || website.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(SupplierErrors.InvalidWebsite);
            }

            if (errors.Any())
            {
                return errors;
            }

            Supplier supplier = new()
            {
                Name = name,
                Description = description,
                Website = website
            };

            return supplier;
        }
    }
}
