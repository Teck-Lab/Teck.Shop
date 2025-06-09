using System;
using System.Threading.Tasks;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Catalog.Domain.Entities.PromotionAggregate;
using Catalog.Domain.Entities.SupplierAggregate;
using Catalog.Infrastructure.IntegrationTests.Shared;

namespace Catalog.Infrastructure.IntegrationTests.Brands
{
    public class BrandRepositoryIntegrationTests : BaseEfRepoTestFixture<AppDbContext>
    {
        private BrandRepository _repository = null!;

        protected override AppDbContext CreateDbContext(DbContextOptions<AppDbContext> options)
        {
            var ctx = new AppDbContext(options)
            {
                Products = null!,
                Categories = null!,
                ProductPrices = null!,
                ProductPriceTypes = null!,
                Promotions = null!,
                Suppliers = null!
            };
            ctx.Products = ctx.Set<Product>();
            ctx.Categories = ctx.Set<Category>();
            ctx.ProductPrices = ctx.Set<ProductPrice>();
            ctx.ProductPriceTypes = ctx.Set<ProductPriceType>();
            ctx.Promotions = ctx.Set<Promotion>();
            ctx.Suppliers = ctx.Set<Supplier>();
            return ctx;
        }

        public override async ValueTask InitializeAsync()
        {
            await base.InitializeAsync();
            var httpContextAccessor = new HttpContextAccessor();
            _repository = new BrandRepository(DbContext, httpContextAccessor);
        }

        [Fact]
        public async Task AddAndGetBrand_Works()
        {
            // Arrange
            var brandResult = Brand.Create("TestBrand", "desc", "https://test.com");
            var brand = brandResult.Value;

            // Act
            await _repository.AddAsync(brand, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe("TestBrand");
        }
    }
}
