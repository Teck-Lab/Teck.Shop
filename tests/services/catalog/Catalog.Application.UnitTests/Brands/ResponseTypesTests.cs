using System;
using Catalog.Application.Features.Brands.Dtos;
using Xunit;

namespace Catalog.Application.UnitTests.Brands
{
    public class ResponseTypesTests
    {
        [Fact]
        public void BrandResponse_Defaults_Are_Correct()
        {
            var resp = new BrandResponse();
            Assert.Equal(Guid.Empty, resp.Id);
            Assert.Equal(string.Empty, resp.Name);
            Assert.Null(resp.Description);
            Assert.Null(resp.Website);
        }

        [Fact]
        public void BrandResponse_CanSet_All_Properties()
        {
            var id = Guid.NewGuid();
            var resp = new BrandResponse
            {
                Id = id,
                Name = "Brand",
                Description = "desc",
                Website = "https://brand.com"
            };
            Assert.Equal(id, resp.Id);
            Assert.Equal("Brand", resp.Name);
            Assert.Equal("desc", resp.Description);
            Assert.Equal("https://brand.com", resp.Website);
        }

        [Fact]
        public void BrandResponse_Allows_Nulls()
        {
            var resp = new BrandResponse
            {
                Description = null,
                Website = null
            };
            Assert.Null(resp.Description);
            Assert.Null(resp.Website);
        }
    }
}
