using FastEndpoints;
using FluentValidation;

namespace Catalog.Application.Features.Brands.GetBrand
{
    /// <summary>
    /// The get brand validator.
    /// </summary>
    public sealed class GetBrandValidator : Validator<GetBrandRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetBrandValidator"/> class.
        /// </summary>
        public GetBrandValidator()
        {
            RuleFor(brand => brand.Id)
                .NotEmpty();
        }
    }
}
