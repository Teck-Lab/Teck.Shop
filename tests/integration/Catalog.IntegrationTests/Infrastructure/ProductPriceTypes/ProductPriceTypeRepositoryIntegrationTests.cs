using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using Catalog.IntegrationTests.Shared;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.PromotionAggregate;
using Catalog.Domain.Entities.SupplierAggregate;

namespace Catalog.IntegrationTests.Infrastructure.ProductPriceTypes
{
    public class ProductPriceTypeRepositoryIntegrationTests : BaseEfRepoTestFixture<AppDbContext>
    {
        private ProductPriceTypeRepository _repository = null!;

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
            _repository = new ProductPriceTypeRepository(DbContext, httpContextAccessor);
        }

        [Fact]
        public async Task AddAndGetProductPriceType_Works()
        {
            // Arrange
            var priceTypeResult = ProductPriceType.Create("Retail", 1);
            var priceType = priceTypeResult.Value;

            // Act
            await _repository.AddAsync(priceType, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            var fetched = await _repository.FindByIdAsync(priceType.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe("Retail");
            fetched.Priority.ShouldBe(1);
        }
    }
}
