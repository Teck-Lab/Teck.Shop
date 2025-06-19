using System;
using System.Collections.Generic;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Products.Response;
using Catalog.Application.Features.Categories.Response;
using Catalog.Application.Features.ProductPrices.Response;
using Catalog.Application.Features.Promotions.Response;
using Xunit;

namespace Catalog.UnitTests.Application.Products
{
    public class ResponseTypesTests
    {
        [Fact]
        public void ProductResponse_Defaults_Are_Correct()
        {
            var resp = new ProductResponse();
            Assert.Equal(Guid.Empty, resp.Id);
            Assert.Equal(string.Empty, resp.Name);
            Assert.Null(resp.Description);
            Assert.Equal(string.Empty, resp.Slug);
            Assert.False(resp.IsActive);
            Assert.Equal(string.Empty, resp.ProductSKU);
            Assert.Null(resp.GTIN);
            Assert.Null(resp.BrandId);
            Assert.Null(resp.Brand);
            Assert.Empty(resp.Categories);
            Assert.Empty(resp.ProductPrices);
            Assert.Empty(resp.Promotions);
        }

        [Fact]
        public void ProductResponse_CanSet_All_Properties()
        {
            var id = Guid.NewGuid();
            var brandId = Guid.NewGuid();
            var brand = new BrandResponse { Id = brandId, Name = "Brand" };
            var categories = new List<CategoryResponse> { new CategoryResponse { Name = "Cat" } };
            var prices = new List<ProductPriceResponse> { new ProductPriceResponse { SalePrice = 9.99m, CurrencyCode = "USD" } };
            var promotions = new List<PromotionResponse> { new PromotionResponse { Name = "Promo", Description = "desc", ValidTo = DateTimeOffset.UtcNow } };
            var resp = new ProductResponse
            {
                Id = id,
                Name = "Name",
                Description = "desc",
                Slug = "slug",
                IsActive = true,
                ProductSKU = "sku",
                GTIN = "gtin",
                BrandId = brandId,
                Brand = brand,
                Categories = categories,
                ProductPrices = prices,
                Promotions = promotions
            };
            Assert.Equal(id, resp.Id);
            Assert.Equal("Name", resp.Name);
            Assert.Equal("desc", resp.Description);
            Assert.Equal("slug", resp.Slug);
            Assert.True(resp.IsActive);
            Assert.Equal("sku", resp.ProductSKU);
            Assert.Equal("gtin", resp.GTIN);
            Assert.Equal(brandId, resp.BrandId);
            Assert.Equal(brand, resp.Brand);
            Assert.Equal(categories, resp.Categories);
            Assert.Equal(prices, resp.ProductPrices);
            Assert.Equal(promotions, resp.Promotions);
        }

        [Fact]
        public void ProductResponse_Allows_Nulls_And_Empty_Collections()
        {
            var resp = new ProductResponse
            {
                Description = null,
                GTIN = null,
                Brand = null,
                Categories = new List<CategoryResponse>(),
                ProductPrices = new List<ProductPriceResponse>(),
                Promotions = new List<PromotionResponse>()
            };
            Assert.Null(resp.Description);
            Assert.Null(resp.GTIN);
            Assert.Null(resp.Brand);
            Assert.Empty(resp.Categories);
            Assert.Empty(resp.ProductPrices);
            Assert.Empty(resp.Promotions);
        }
    }
}
