#nullable enable
using System;
using System.Threading.Tasks;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Infrastructure.Caching;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using Catalog.IntegrationTests.Shared;
using ZiggyCreatures.Caching.Fusion;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Catalog.Domain.Entities.PromotionAggregate;
using Catalog.Domain.Entities.SupplierAggregate;
using Teck.Shop.SharedKernel.Persistence;
using Catalog.Application.Contracts.Repositories;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Infrastructure.Products
{
    [Collection("SharedTestcontainers")]
    public class ProductCacheIntegrationTests : BaseCacheTestFixture<AppDbContext>
    {
        private ProductCache _cache = null!;
        private IProductRepository _repository = null!;
        private IUnitOfWork _unitOfWork = null!;

        public ProductCacheIntegrationTests(SharedTestcontainersFixture sharedFixture)
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
        public override async ValueTask InitializeAsync()
        {
            await base.InitializeAsync();
            var httpContextAccessor = new HttpContextAccessor();
            _repository = new ProductRepository(DbContext, httpContextAccessor);
            var publishEndpoint = ServiceProvider.GetRequiredService<MassTransit.IPublishEndpoint>();
            _unitOfWork = new UnitOfWork<AppDbContext>(DbContext);
            _cache = new ProductCache(Cache, _repository);
        }

        // DisposeAsync is handled by base class

        [Fact]
        public async Task GetOrSetByIdAsync_Should_ReturnProduct_When_ProductExists()
        {
            // Arrange
            var brandResult = Catalog.Domain.Entities.BrandAggregate.Brand.Create("Brand for Product", "desc", "https://brand.com");
            var brand = brandResult.Value;
            var categoryResult = Catalog.Domain.Entities.CategoryAggregate.Category.Create("Test Category", "desc");
            var category = categoryResult.Value;
            var brandRepository = new BrandRepository(DbContext, new HttpContextAccessor());
            var categoryRepository = new CategoryRepository(DbContext, new HttpContextAccessor());
            await brandRepository.AddAsync(brand, TestContext.Current.CancellationToken);
            await categoryRepository.AddAsync(category, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            var trackedBrand = await brandRepository.FindByIdAsync(brand.Id, true, TestContext.Current.CancellationToken);
            var trackedCategory = await categoryRepository.FindByIdAsync(category.Id, true, TestContext.Current.CancellationToken);
            var productResult = Catalog.Domain.Entities.ProductAggregate.Product.Create("Test Product", "desc", "sku1", "gtin1", new List<Catalog.Domain.Entities.CategoryAggregate.Category> { trackedCategory! }, true, trackedBrand!);
            var product = productResult.Value;
            await _repository.AddAsync(product, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _cache.GetOrSetByIdAsync(product.Id, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result!.Name.ShouldBe("Test Product");
        }

        [Fact]
        public async Task SetAsync_Should_StoreProductInCache()
        {
            // Arrange
            var brandResult = Catalog.Domain.Entities.BrandAggregate.Brand.Create("Brand for Product", "desc", "https://brand.com");
            var brand = brandResult.Value;
            var categoryResult = Catalog.Domain.Entities.CategoryAggregate.Category.Create("Test Category", "desc");
            var category = categoryResult.Value;
            var brandRepository = new BrandRepository(DbContext, new HttpContextAccessor());
            var categoryRepository = new CategoryRepository(DbContext, new HttpContextAccessor());
            await brandRepository.AddAsync(brand, TestContext.Current.CancellationToken);
            await categoryRepository.AddAsync(category, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            var trackedBrand = await brandRepository.FindByIdAsync(brand.Id, true, TestContext.Current.CancellationToken);
            var trackedCategory = await categoryRepository.FindByIdAsync(category.Id, true, TestContext.Current.CancellationToken);
            var productResult = Catalog.Domain.Entities.ProductAggregate.Product.Create("Cache Product", "desc", "sku1", "gtin1", new List<Catalog.Domain.Entities.CategoryAggregate.Category> { trackedCategory! }, true, trackedBrand!);
            var product = productResult.Value;
            await _repository.AddAsync(product, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            await _cache.SetAsync(product.Id, product, TestContext.Current.CancellationToken);
            var result = await _cache.GetOrSetByIdAsync(product.Id, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result!.Name.ShouldBe("Cache Product");
        }

        [Fact]
        public async Task RemoveAsync_Should_RemoveProductFromCache()
        {
            // Arrange
            var brandResult = Catalog.Domain.Entities.BrandAggregate.Brand.Create("Brand for Product", "desc", "https://brand.com");
            var brand = brandResult.Value;
            var categoryResult = Catalog.Domain.Entities.CategoryAggregate.Category.Create("Test Category", "desc");
            var category = categoryResult.Value;
            var brandRepository = new BrandRepository(DbContext, new HttpContextAccessor());
            var categoryRepository = new CategoryRepository(DbContext, new HttpContextAccessor());
            await brandRepository.AddAsync(brand, TestContext.Current.CancellationToken);
            await categoryRepository.AddAsync(category, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            var trackedBrand = await brandRepository.FindByIdAsync(brand.Id, true, TestContext.Current.CancellationToken);
            var trackedCategory = await categoryRepository.FindByIdAsync(category.Id, true, TestContext.Current.CancellationToken);
            var productResult = Catalog.Domain.Entities.ProductAggregate.Product.Create("Remove Product", "desc", "sku2", "gtin2", new List<Catalog.Domain.Entities.CategoryAggregate.Category> { trackedCategory! }, true, trackedBrand!);
            var product = productResult.Value;
            await _repository.AddAsync(product, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

            await _cache.SetAsync(product.Id, product, TestContext.Current.CancellationToken);
            // Remove from database as well to prevent cache repopulation
            _repository.Delete(product);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            await _cache.RemoveAsync(product.Id, TestContext.Current.CancellationToken);
            var result = await _cache.GetOrSetByIdAsync(product.Id, cancellationToken: TestContext.Current.CancellationToken);
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ExpireAsync_Should_ExpireProductFromCache()
        {
            // Arrange
            var brandResult = Catalog.Domain.Entities.BrandAggregate.Brand.Create("Brand for Product", "desc", "https://brand.com");
            var brand = brandResult.Value;
            var categoryResult = Catalog.Domain.Entities.CategoryAggregate.Category.Create("Test Category", "desc");
            var category = categoryResult.Value;
            var brandRepository = new BrandRepository(DbContext, new HttpContextAccessor());
            var categoryRepository = new CategoryRepository(DbContext, new HttpContextAccessor());
            await brandRepository.AddAsync(brand, TestContext.Current.CancellationToken);
            await categoryRepository.AddAsync(category, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            var trackedBrand = await brandRepository.FindByIdAsync(brand.Id, true, TestContext.Current.CancellationToken);
            var trackedCategory = await categoryRepository.FindByIdAsync(category.Id, true, TestContext.Current.CancellationToken);
            var productResult = Catalog.Domain.Entities.ProductAggregate.Product.Create("Expire Product", "desc", "sku3", "gtin3", new List<Catalog.Domain.Entities.CategoryAggregate.Category> { trackedCategory! }, true, trackedBrand!);
            var product = productResult.Value;
            await _repository.AddAsync(product, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

            await _cache.SetAsync(product.Id, product, TestContext.Current.CancellationToken);
            // Remove from database as well to prevent cache repopulation
            _repository.Delete(product);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            await _cache.ExpireAsync(product.Id, TestContext.Current.CancellationToken);
            var result = await _cache.GetOrSetByIdAsync(product.Id, cancellationToken: TestContext.Current.CancellationToken);
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetOrSetByIdAsync_Should_ReturnNull_When_ProductDoesNotExist()
        {
            var result = await _cache.GetOrSetByIdAsync(Guid.NewGuid(), cancellationToken: TestContext.Current.CancellationToken);
            result.ShouldBeNull();
        }

        // Additional tests for cache-miss scenarios can be added here.
    }
}
