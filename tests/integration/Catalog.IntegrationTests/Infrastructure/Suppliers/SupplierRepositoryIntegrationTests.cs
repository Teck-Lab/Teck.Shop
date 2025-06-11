using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Entities.SupplierAggregate;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using Catalog.IntegrationTests.Shared;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Catalog.Domain.Entities.PromotionAggregate;

namespace Catalog.IntegrationTests.Infrastructure.Suppliers
{
    public class SupplierRepositoryIntegrationTests : BaseEfRepoTestFixture<AppDbContext>
    {
        private SupplierRepository _repository = null!;

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
            _repository = new SupplierRepository(DbContext, httpContextAccessor);
        }

        [Fact]
        public async Task AddAndGetSupplier_Works()
        {
            // Arrange
            var supplierResult = Supplier.Create("TestSupplier", "desc", "https://supplier.com");
            var supplier = supplierResult.Value;

            // Act
            await _repository.AddAsync(supplier, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            var fetched = await _repository.FindByIdAsync(supplier.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe("TestSupplier");
            fetched.Website.ShouldBe("https://supplier.com");
        }
    }
}
