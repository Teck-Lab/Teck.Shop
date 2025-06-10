#nullable enable
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

        [Theory]
        [InlineData("BrandA", "descA", "https://a.com")]
        [InlineData("BrandB", "descB", "https://b.com")]
        [InlineData("BrandC", "descC", "https://c.com")]
        public async Task AddAndGetBrand_Works_WithVariousData(string name, string desc, string website)
        {
            // Arrange
            var brandResult = Brand.Create(name, desc, website);
            var brand = brandResult.Value;

            // Act
            await _repository.AddAsync(brand, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe(name);
            fetched.Description.ShouldBe(desc);
            fetched.Website.ShouldBe(website);
        }

        [Fact]
        public async Task UpdateBrand_Works()
        {
            // Arrange
            var brandResult = Brand.Create("BrandToUpdate", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            brand.Update("UpdatedName", "UpdatedDesc", "https://updated.com");
            _repository.Update(brand);
            await DbContext.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe("UpdatedName");
            fetched.Description.ShouldBe("UpdatedDesc");
            fetched.Website.ShouldBe("https://updated.com");
        }

        [Theory]
        [InlineData("PartialUpdateBrand", "desc", "https://partial.com", null, "newdesc", null, "PartialUpdateBrand", "newdesc", "https://partial.com")]
        [InlineData("PartialUpdateBrand2", "desc2", "https://partial2.com", "NewName", null, null, "NewName", "desc2", "https://partial2.com")]
        [InlineData("PartialUpdateBrand3", "desc3", "https://partial3.com", null, null, "https://changed.com", "PartialUpdateBrand3", "desc3", "https://changed.com")]
        public async Task UpdateBrand_WithNullsOrUnchanged_OnlyUpdatesProvided(string initialName, string initialDesc, string initialWebsite, string? newName, string? newDesc, string? newWebsite, string expectedName, string expectedDesc, string? expectedWebsite)
        {
            // Arrange
            var brandResult = Brand.Create(initialName, initialDesc, initialWebsite);
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            brand.Update(newName, newDesc, newWebsite);
            _repository.Update(brand);
            await DbContext.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe(expectedName);
            fetched.Description.ShouldBe(expectedDesc);
            fetched.Website.ShouldBe(expectedWebsite);
        }

        [Theory]
        [InlineData(null, "newdesc", null)]
        [InlineData("NewName", null, null)]
        [InlineData(null, null, "https://newsite.com")]
        [InlineData("NewName", "newdesc", "https://newsite.com")]
        public async Task UpdateBrand_WithNulls_DoesNotChangeUnchangedFields_Theory(string? newName, string? newDesc, string? newWebsite)
        {
            // Arrange
            var brandResult = Brand.Create("PartialUpdateBrand", "desc", "https://partial.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            brand.Update(newName, newDesc, newWebsite);
            _repository.Update(brand);
            await DbContext.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe(newName ?? "PartialUpdateBrand");
            fetched.Description.ShouldBe(newDesc ?? "desc");
            fetched.Website.ShouldBe(newWebsite ?? "https://partial.com");
        }

        [Fact]
        public async Task DeleteBrand_Works()
        {
            // Arrange
            var brandResult = Brand.Create("BrandToDelete", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            _repository.Delete(brand);
            await DbContext.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldBeNull();
        }

        [Theory]
        [InlineData("GhostBrand", "desc", "https://ghost.com")]
        [InlineData("GhostBrand2", "desc2", null)]
        public void DeleteBrand_NonExistent_DoesNotThrow_Theory(string name, string desc, string? website)
        {
            // Arrange
            var brandResult = Brand.Create(name, desc, website);
            var brand = brandResult.Value;
            // Not added to DB

            // Act & Assert
            Should.NotThrow(() => _repository.Delete(brand));
        }

        [Fact]
        public async Task AddBrand_WithNullWebsite_Works()
        {
            // Arrange
            var brandResult = Brand.Create("NullWebsiteBrand", "desc", null);
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Website.ShouldBeNull();
        }

        [Theory]
        [InlineData("NullWebsiteBrand", "desc", null)]
        [InlineData("NullWebsiteBrand2", "desc2", null)]
        public async Task AddBrand_WithNullWebsite_Works_Theory(string name, string desc, string? website)
        {
            // Arrange
            var brandResult = Brand.Create(name, desc, website);
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Website.ShouldBeNull();
        }

        [Fact]
        public async Task GetPagedBrandsAsync_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                var brandResult = Brand.Create($"PagedBrand{i}", "desc", $"https://brand{i}.com");
                var brand = brandResult.Value;
                await _repository.AddAsync(brand, CancellationToken.None);
            }
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            var paged = await _repository.GetPagedBrandsAsync(2, 3, null, CancellationToken.None);

            // Assert
            paged.ShouldNotBeNull();
            paged.Items.Count.ShouldBe(3);
            paged.Page.ShouldBe(2);
            paged.Size.ShouldBe(3);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 3)]
        [InlineData(3, 2)]
        public async Task GetPagedBrandsAsync_ReturnsCorrectPage_Theory(int page, int size)
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                var brandResult = Brand.Create($"PagedBrand{i}", "desc", $"https://brand{i}.com");
                var brand = brandResult.Value;
                await _repository.AddAsync(brand, CancellationToken.None);
            }
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            var paged = await _repository.GetPagedBrandsAsync(page, size, null, CancellationToken.None);

            // Assert
            paged.ShouldNotBeNull();
            paged.Items.Count.ShouldBe(size);
            paged.Page.ShouldBe(page);
            paged.Size.ShouldBe(size);
        }

        [Theory]
        [InlineData("Alpha", 5)]
        [InlineData("Beta", 5)]
        [InlineData("PagedBrand", 0)]
        public async Task GetPagedBrandsAsync_WithKeyword_FiltersCorrectly_Theory(string keyword, int expectedCount)
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                var brandResult = Brand.Create($"AlphaBrand{i}", "desc", $"https://alpha{i}.com");
                var brand = brandResult.Value;
                await _repository.AddAsync(brand, CancellationToken.None);
            }
            for (int i = 0; i < 5; i++)
            {
                var brandResult = Brand.Create($"BetaBrand{i}", "desc", $"https://beta{i}.com");
                var brand = brandResult.Value;
                await _repository.AddAsync(brand, CancellationToken.None);
            }
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            var paged = await _repository.GetPagedBrandsAsync(1, 10, keyword, CancellationToken.None);

            // Assert
            paged.ShouldNotBeNull();
            paged.Items.Count.ShouldBe(expectedCount);
            if (expectedCount > 0)
                paged.Items.All(b => b.Name.Contains(keyword)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(1)]
        [InlineData(0)]
        public async Task DeleteBrandsAsync_RemovesMultipleBrands_Theory(int count)
        {
            // Arrange
            var ids = new List<Guid>();
            for (int i = 0; i < count; i++)
            {
                var brandResult = Brand.Create($"DelMany{i}", "desc", $"https://delmany{i}.com");
                var brand = brandResult.Value;
                await _repository.AddAsync(brand, CancellationToken.None);
                ids.Add(brand.Id);
            }
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Act
            await _repository.DeleteBrandsAsync(ids, CancellationToken.None);
            await DbContext.SaveChangesAsync(CancellationToken.None);

            // Assert
            foreach (var id in ids)
            {
                var fetched = await _repository.FindByIdAsync(id, true, CancellationToken.None);
                fetched.ShouldBeNull();
            }
        }

        // Remove the old [Fact] versions of these tests, as they are now covered by the [Theory] versions.
        [Fact]
        public async Task FindByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Act
            var fetched = await _repository.FindByIdAsync(Guid.NewGuid(), true, CancellationToken.None);
            // Assert
            fetched.ShouldBeNull();
        }
    }
}
