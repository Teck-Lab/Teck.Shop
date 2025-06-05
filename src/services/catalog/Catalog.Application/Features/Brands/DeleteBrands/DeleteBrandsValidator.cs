using Catalog.Application.Features.Brands.DeleteBrand;
using FastEndpoints;
using FluentValidation;

namespace Catalog.Application.Features.Brands.DeleteBrands
{
    /// <summary>
    /// The delete brands validator.
    /// </summary>
    public sealed class DeleteBrandsValidator : Validator<DeleteBrandRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteBrandsValidator"/> class.
        /// </summary>
        public DeleteBrandsValidator()
        {
            RuleFor(brand => brand.Id)
                .NotEmpty();
        }
    }
}
