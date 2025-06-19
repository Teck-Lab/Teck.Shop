#nullable enable
using System;
using System.Threading.Tasks;
using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Catalog.Domain.Entities.PromotionAggregate;
using Catalog.Domain.Entities.SupplierAggregate;
using Catalog.Infrastructure.Caching;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using Catalog.IntegrationTests.Shared;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Infrastructure.Brands
{
    [Collection("SharedTestcontainers")]
    public class BrandCacheIntegrationTests : BaseCacheTestFixture<AppDbContext>
    {
        private BrandCache _cache = null!;
        private IBrandRepository _repository = null!;
        private IUnitOfWork _unitOfWork = null!;
        public BrandCacheIntegrationTests(SharedTestcontainersFixture sharedFixture)
            : base(sharedFixture) { }
        public override async ValueTask InitializeAsync()
        {
            await base.InitializeAsync();
            var httpContextAccessor = new HttpContextAccessor();
            _repository = new BrandRepository(DbContext, httpContextAccessor);
            var publishEndpoint = ServiceProvider.GetRequiredService<MassTransit.IPublishEndpoint>();
            _unitOfWork = new UnitOfWork<AppDbContext>(DbContext);
            _cache = new BrandCache(Cache, _repository);
        }

        // DisposeAsync is handled by base class

        [Fact]
        public async Task GetOrSetByIdAsync_Should_ReturnBrand_When_BrandExists()
        {
            var brandResult = Catalog.Domain.Entities.BrandAggregate.Brand.Create("Test Brand", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await _cache.GetOrSetByIdAsync(brand.Id, cancellationToken: TestContext.Current.CancellationToken);
            result.ShouldNotBeNull();
            result!.Name.ShouldBe("Test Brand");
        }

        [Fact]
        public async Task SetAsync_Should_StoreBrandInCache()
        {
            var brandResult = Catalog.Domain.Entities.BrandAggregate.Brand.Create("Cache Brand", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            await _cache.SetAsync(brand.Id, brand, TestContext.Current.CancellationToken);
            var result = await _cache.GetOrSetByIdAsync(brand.Id, cancellationToken: TestContext.Current.CancellationToken);
            result.ShouldNotBeNull();
            result!.Name.ShouldBe("Cache Brand");
        }

        [Fact]
        public async Task RemoveAsync_Should_RemoveBrandFromCache()
        {
            var brandResult = Catalog.Domain.Entities.BrandAggregate.Brand.Create("Remove Brand", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            await _cache.SetAsync(brand.Id, brand, TestContext.Current.CancellationToken);
            await _cache.RemoveAsync(brand.Id, TestContext.Current.CancellationToken);
            _repository.Delete(brand);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            var result = await _cache.GetOrSetByIdAsync(brand.Id, cancellationToken: TestContext.Current.CancellationToken);
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ExpireAsync_Should_ExpireBrandFromCache()
        {
            var brandResult = Catalog.Domain.Entities.BrandAggregate.Brand.Create("Expire Brand", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, TestContext.Current.CancellationToken);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            await _cache.SetAsync(brand.Id, brand, TestContext.Current.CancellationToken);
            await _cache.ExpireAsync(brand.Id, TestContext.Current.CancellationToken);
            _repository.Delete(brand);
            await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            var result = await _cache.GetOrSetByIdAsync(brand.Id, cancellationToken: TestContext.Current.CancellationToken);
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetOrSetByIdAsync_Should_ReturnNull_When_BrandDoesNotExist()
        {
            var result = await _cache.GetOrSetByIdAsync(Guid.NewGuid(), cancellationToken: TestContext.Current.CancellationToken);
            result.ShouldBeNull();
        }

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
    }
}
