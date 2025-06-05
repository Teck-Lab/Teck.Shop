using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalog.Application.Features.Brands.CreateBrand.V1;
using Catalog.Application.Features.Brands.GetPaginatedBrands;
using FastEndpoints;
using FluentValidation;

namespace Catalog.Application.Features.Brands.GetPaginatedBrands
{
    /// <summary>
    /// The validator for Pagianted brands.
    /// </summary>
    public sealed class GetPaginatedBrandsValidator : Validator<GetPaginatedBrandsRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBrandValidator"/> class.
        /// </summary>
        public GetPaginatedBrandsValidator()
        {
            RuleFor(brand => brand.Page)
                .NotEmpty()
                .GreaterThan(0);
            RuleFor(brand => brand.Size)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}
