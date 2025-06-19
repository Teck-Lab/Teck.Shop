using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Entities.PromotionAggregate;
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
using Catalog.Domain.Entities.SupplierAggregate;
using Catalog.Domain.Entities.BrandAggregate;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Infrastructure.Promotions
{
    [Collection("SharedTestcontainers")]
    public class PromotionRepositoryIntegrationTests : BaseEfRepoTestFixture<AppDbContext, IUnitOfWork>
    {
        private PromotionRepository _repository = null!;
        private ProductRepository _productRepository = null!;
        private BrandRepository _brandRepository = null!;
        private CategoryRepository _categoryRepository = null!;
        public PromotionRepositoryIntegrationTests(SharedTestcontainersFixture sharedFixture)
        : base(sharedFixture) { }
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

        protected override IUnitOfWork CreateUnitOfWork(AppDbContext context)
        {
            var publishEndpoint = ServiceProvider.GetRequiredService<MassTransit.IPublishEndpoint>();
            return new UnitOfWork<AppDbContext>(context);
        }

        public override async ValueTask InitializeAsync()
        {
            await base.InitializeAsync();
            var httpContextAccessor = new HttpContextAccessor();
            _repository = new PromotionRepository(DbContext, httpContextAccessor);
            _productRepository = new ProductRepository(DbContext, httpContextAccessor);
            _brandRepository = new BrandRepository(DbContext, httpContextAccessor);
            _categoryRepository = new CategoryRepository(DbContext, httpContextAccessor);
        }

        [Fact]
        public async Task AddAndGetPromotion_Works()
        {
            // Arrange: create and persist a brand, category, and product first
            var brandResult = Brand.Create("TestBrand", "desc", "https://test.com");
            var brand = brandResult.Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);

            var categoryResult = Category.Create("TestCategory", "desc");
            var category = categoryResult.Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);

            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            var productResult = Product.Create(
                "TestProduct",
                "desc",
                "SKU123",
                "GTIN123",
                new List<Category> { category },
                true,
                brand
            );
            var product = productResult.Value;
            await _productRepository.AddAsync(product, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act: create promotion
            var promoResult = Promotion.Create(
                "TestPromo",
                "desc",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(10),
                new List<Product> { product }
            );
            var promo = promoResult.Value;
            await _repository.AddAsync(promo, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            var fetched = await _repository.FindByIdAsync(promo.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe("TestPromo");
            fetched.Products.ShouldContain(p => p.Id == product.Id);
        }
    }
}
