using System;
using System.Collections.Generic;
using Catalog.Application.Features.Brands.CreateBrand.V1;
using Catalog.Application.Features.Brands.UpdateBrand;
using Catalog.Application.Features.Brands.DeleteBrand;
using Catalog.Application.Features.Brands.DeleteBrands;
using Catalog.Application.Features.Brands.GetBrand;
using Catalog.Application.Features.Brands.GetPaginatedBrands;
using Xunit;

namespace Catalog.UnitTests.Application.Brands
{
    public class RequestTypesTests
    {
        [Fact]
        public void CreateBrandRequest_CanSetProperties()
        {
            var req = new CreateBrandRequest
            {
                Name = "Test Brand",
                Description = "desc",
                Website = "https://brand.com"
            };
            Assert.Equal("Test Brand", req.Name);
            Assert.Equal("desc", req.Description);
            Assert.Equal("https://brand.com", req.Website);
        }

        [Fact]
        public void UpdateBrandRequest_CanSetProperties()
        {
            var id = Guid.NewGuid();
            var req = new UpdateBrandRequest
            {
                Id = id,
                Name = "Name",
                Description = "desc",
                Website = "https://brand.com"
            };
            Assert.Equal(id, req.Id);
            Assert.Equal("Name", req.Name);
            Assert.Equal("desc", req.Description);
            Assert.Equal("https://brand.com", req.Website);
        }

        [Fact]
        public void DeleteBrandRequest_CanSetId()
        {
            var id = Guid.NewGuid();
            var req = new DeleteBrandRequest { Id = id };
            Assert.Equal(id, req.Id);
        }

        [Fact]
        public void DeleteBrandsRequest_CanSetIds()
        {
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var req = new DeleteBrandsRequest { Ids = ids };
            Assert.Equal(ids, req.Ids);
        }

        [Fact]
        public void GetBrandRequest_CanSetId()
        {
            var id = Guid.NewGuid();
            var req = new GetBrandRequest { Id = id };
            Assert.Equal(id, req.Id);
        }

        [Fact]
        public void GetPaginatedBrandsRequest_CanSetKeyword()
        {
            var req = new GetPaginatedBrandsRequest { Keyword = "search" };
            Assert.Equal("search", req.Keyword);
        }

        [Fact]
        public void CreateBrandRequest_Default_Properties()
        {
            var req = new CreateBrandRequest();
            Assert.Equal(string.Empty, req.Name);
            Assert.Null(req.Description);
            Assert.Null(req.Website);
        }

        [Fact]
        public void DeleteBrandRequest_Default_Properties()
        {
            var req = new DeleteBrandRequest();
            Assert.Equal(Guid.Empty, req.Id);
        }

        [Fact]
        public void DeleteBrandsRequest_Default_Properties()
        {
            var req = new DeleteBrandsRequest();
            Assert.Empty(req.Ids);
        }

        [Fact]
        public void GetBrandRequest_Default_Properties()
        {
            var req = new GetBrandRequest();
            Assert.Equal(Guid.Empty, req.Id);
        }

        [Fact]
        public void GetPaginatedBrandsRequest_Default_Properties()
        {
            var req = new GetPaginatedBrandsRequest();
            Assert.Null(req.Keyword);
        }

        [Fact]
        public void UpdateBrandRequest_Default_Properties()
        {
            var req = new UpdateBrandRequest();
            Assert.Equal(Guid.Empty, req.Id);
            Assert.Null(req.Name);
            Assert.Null(req.Description);
            Assert.Null(req.Website);
        }
    }
}
