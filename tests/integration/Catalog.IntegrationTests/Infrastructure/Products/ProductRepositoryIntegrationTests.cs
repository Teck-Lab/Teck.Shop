#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Catalog.Domain.Entities.PromotionAggregate;
using Catalog.Domain.Entities.SupplierAggregate;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using Catalog.IntegrationTests.Shared;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Infrastructure.Products
{
    [Collection("SharedTestcontainers")]
    public class ProductRepositoryIntegrationTests : BaseEfRepoTestFixture<AppDbContext, IUnitOfWork>
    {
        private ProductRepository _repository = null!;
        private BrandRepository _brandRepository = null!;
        private CategoryRepository _categoryRepository = null!;
        public ProductRepositoryIntegrationTests(SharedTestcontainersFixture sharedFixture)
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
            _repository = new ProductRepository(DbContext, httpContextAccessor);
            _brandRepository = new BrandRepository(DbContext, httpContextAccessor);
            _categoryRepository = new CategoryRepository(DbContext, httpContextAccessor);
        }

        [Fact]
        public async Task AddAndGetProduct_Works()
        {
            // Arrange: create and persist a brand and category first
            var brandResult = Brand.Create("TestBrand", "desc", "https://test.com");
            var brand = brandResult.Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);

            var categoryResult = Category.Create("TestCategory", "desc");
            var category = categoryResult.Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);

            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act: create product
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
            await _repository.AddAsync(product, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            var fetched = await _repository.FindByIdAsync(product.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe("TestProduct");
            fetched.BrandId.ShouldBe(brand.Id);
            fetched.Categories.ShouldContain(c => c.Id == category.Id);
        }

        [Fact]
        public async Task UpdateProduct_Works()
        {
            // Arrange
            var brandResult = Brand.Create("BrandToUpdate", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);
            var categoryResult = Category.Create("CategoryToUpdate", "desc");
            var category = categoryResult.Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var productResult = Product.Create("ProductToUpdate", "desc", "SKU1", "GTIN1", new List<Category> { category }, true, brand);
            var product = productResult.Value;
            await _repository.AddAsync(product, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            product.Update("UpdatedName");
            _repository.Update(product);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(product.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe("UpdatedName");
        }

        [Fact]
        public async Task DeleteProduct_Works()
        {
            // Arrange
            var brandResult = Brand.Create("BrandToDelete", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);
            var categoryResult = Category.Create("CategoryToDelete", "desc");
            var category = categoryResult.Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var productResult = Product.Create("ProductToDelete", "desc", "SKU2", "GTIN2", new List<Category> { category }, true, brand);
            var product = productResult.Value;
            await _repository.AddAsync(product, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            _repository.Delete(product);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(product.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldBeNull();
        }

        [Fact]
        public async Task GetPagedProductsAsync_ReturnsCorrectPage()
        {
            // Arrange
            var brandResult = Brand.Create("BrandPaged", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);
            var categoryResult = Category.Create("CategoryPaged", "desc");
            var category = categoryResult.Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            for (int i = 0; i < 10; i++)
            {
                var productResult = Product.Create($"PagedProduct{i}", "desc", $"SKU{i}", $"GTIN{i}", new List<Category> { category }, true, brand);
                var product = productResult.Value;
                await _repository.AddAsync(product, CancellationToken.None);
            }
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            var paged = await _repository.GetPagedProductsAsync(2, 3, null, CancellationToken.None);

            // Assert
            paged.ShouldNotBeNull();
            paged.Items.Count.ShouldBe(3);
            paged.Page.ShouldBe(2);
            paged.Size.ShouldBe(3);
        }

        [Fact]
        public async Task FindByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Act
            var fetched = await _repository.FindByIdAsync(Guid.NewGuid(), true, CancellationToken.None);
            // Assert
            fetched.ShouldBeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AddProduct_WithNullOrEmptyGtin_Works(string? gtin)
        {
            var brand = Brand.Create("BrandNullGtin", "desc", "https://brand.com").Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);
            var category = Category.Create("CategoryNullGtin", "desc").Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var productResult = Product.Create("ProductNullGtin", "desc", "SKU-NULLGTIN", gtin, new List<Category> { category }, true, brand);
            var product = productResult.Value;
            await _repository.AddAsync(product, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(product.Id, true, CancellationToken.None);
            fetched.ShouldNotBeNull();
            fetched!.GTIN.ShouldBe(gtin);
        }

        [Fact]
        public async Task AddProduct_WithMultipleCategories_Works()
        {
            var brand = Brand.Create("BrandMultiCat", "desc", "https://brand.com").Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);
            var cat1 = Category.Create("Cat1", "desc").Value;
            var cat2 = Category.Create("Cat2", "desc").Value;
            await _categoryRepository.AddAsync(cat1, CancellationToken.None);
            await _categoryRepository.AddAsync(cat2, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var productResult = Product.Create("ProductMultiCat", "desc", "SKU-MULTICAT", "GTIN-MULTICAT", new List<Category> { cat1, cat2 }, true, brand);
            var product = productResult.Value;
            await _repository.AddAsync(product, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(product.Id, true, CancellationToken.None);
            fetched.ShouldNotBeNull();
            fetched!.Categories.Count.ShouldBe(2);
            fetched.Categories.ShouldContain(c => c.Id == cat1.Id);
            fetched.Categories.ShouldContain(c => c.Id == cat2.Id);
        }

        [Theory]
        [InlineData("UpdatedName")]
        [InlineData(null)]
        public async Task UpdateProduct_PartialUpdate_Works(string? newName)
        {
            var brand = Brand.Create("BrandPartialUpdate", "desc", "https://brand.com").Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);
            var category = Category.Create("CatPartialUpdate", "desc").Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var productResult = Product.Create("ProductPartialUpdate", "desc", "SKU-PARTIAL", "GTIN-PARTIAL", new List<Category> { category }, true, brand);
            var product = productResult.Value;
            await _repository.AddAsync(product, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            product.Update(newName ?? product.Name);
            _repository.Update(product);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(product.Id, true, CancellationToken.None);
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe(newName ?? "ProductPartialUpdate");
        }

        [Fact]
        public void DeleteProduct_NonExistent_DoesNotThrow()
        {
            var brand = Brand.Create("BrandGhost", "desc", "https://brand.com").Value;
            var category = Category.Create("CatGhost", "desc").Value;
            var productResult = Product.Create("GhostProduct", "desc", "SKU-GHOST", "GTIN-GHOST", new List<Category> { category }, true, brand);
            var product = productResult.Value;
            Should.NotThrow(() => _repository.Delete(product));
        }

        [Theory]
        [InlineData("PagedProduct", 5)]
        [InlineData("NonExistent", 0)]
        public async Task GetPagedProductsAsync_WithKeyword_FiltersCorrectly(string keyword, int expectedCount)
        {
            // Ensure clean state
            DbContext.Products.RemoveRange(DbContext.Products);
            await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

            var brand = Brand.Create("BrandPagedKeyword", "desc", "https://brand.com").Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);
            var category = Category.Create("CatPagedKeyword", "desc").Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            for (int i = 0; i < 5; i++)
            {
                var productResult = Product.Create($"PagedProduct{i}", "desc", $"SKU-KW{i}", $"GTIN-KW{i}", new List<Category> { category }, true, brand);
                var product = productResult.Value;
                await _repository.AddAsync(product, CancellationToken.None);
            }
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var paged = await _repository.GetPagedProductsAsync(1, 10, keyword, CancellationToken.None);
            paged.ShouldNotBeNull();
            paged.Items.Count.ShouldBe(expectedCount);
            if (expectedCount > 0)
                paged.Items.All(p => p.Name.Contains(keyword)).ShouldBeTrue();
        }

        [Fact]
        public async Task DeleteMultipleProducts_OnlyDeletesSpecified()
        {
            var brand = Brand.Create("BrandDelMany", "desc", "https://brand.com").Value;
            await _brandRepository.AddAsync(brand, CancellationToken.None);
            var category = Category.Create("CatDelMany", "desc").Value;
            await _categoryRepository.AddAsync(category, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var ids = new List<Guid>();
            for (int i = 0; i < 3; i++)
            {
                var productResult = Product.Create($"DelMany{i}", "desc", $"SKU-DEL{i}", $"GTIN-DEL{i}", new List<Category> { category }, true, brand);
                var product = productResult.Value;
                await _repository.AddAsync(product, CancellationToken.None);
                ids.Add(product.Id);
            }
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            foreach (var id in ids)
            {
                var prod = await _repository.FindByIdAsync(id, true, CancellationToken.None);
                _repository.Delete(prod!);
            }
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            foreach (var id in ids)
            {
                var fetched = await _repository.FindByIdAsync(id, true, CancellationToken.None);
                fetched.ShouldBeNull();
            }
        }
    }
}
