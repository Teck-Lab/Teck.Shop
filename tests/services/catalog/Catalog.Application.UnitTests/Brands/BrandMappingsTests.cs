using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.Mappings;
using Catalog.Domain.Entities.BrandAggregate;
using Teck.Shop.SharedKernel.Core.Pagination;
using System;
using System.Collections.Generic;
using Xunit;

namespace Catalog.Application.UnitTests.Brands
{
    public class BrandMappingsTests
    {
        [Fact]
        public void BrandToBrandResponse_Maps_All_Properties()
        {
            var brand = new Brand();
            var response = BrandMapper.BrandToBrandResponse(brand);
            Assert.NotNull(response);
        }

        [Fact]
        public void PagedBrandToPagedBrandResponse_Maps_All_Properties()
        {
            var brands = new PagedList<Brand>(new List<Brand> { new Brand() }, 1, 1, 1);
            var pagedResponse = BrandMapper.PagedBrandToPagedBrandResponse(brands);
            Assert.NotNull(pagedResponse);
            Assert.Single(pagedResponse.Items);
        }
    }
}
