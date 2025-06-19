#nullable enable
using System;
using System.Threading.Tasks;
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
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.BrandAggregate.Errors;
using Catalog.Domain.Entities.BrandAggregate.ValueObjects;
using Catalog.IntegrationTests.Shared;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;
using Microsoft.Extensions.DependencyInjection;
using ErrorOr;

namespace Catalog.IntegrationTests.Infrastructure.Brands
{
    [Collection("SharedTestcontainers")]
    public class BrandRepositoryIntegrationTests : BaseEfRepoTestFixture<AppDbContext, IUnitOfWork>
    {
        public BrandRepositoryIntegrationTests(SharedTestcontainersFixture sharedFixture)
            : base(sharedFixture) { }

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

        protected override IUnitOfWork CreateUnitOfWork(AppDbContext context)
        {
            var publishEndpoint = ServiceProvider.GetRequiredService<MassTransit.IPublishEndpoint>();
            return new UnitOfWork<AppDbContext>(context);
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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe(name);
            fetched.Description.ShouldBe(desc);
            fetched.Website?.ToString().ShouldBe(website);
        }

        [Fact]
        public async Task UpdateBrand_Works()
        {
            // Arrange
            var brandResult = Brand.Create("BrandToUpdate", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            brand.Update("UpdatedName", "UpdatedDesc", "https://updated.com");
            _repository.Update(brand);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe("UpdatedName");
            fetched.Description.ShouldBe("UpdatedDesc");
            fetched.Website?.ToString().ShouldBe("https://updated.com");
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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            brand.Update(newName, newDesc, newWebsite);
            _repository.Update(brand);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe(expectedName);
            fetched.Description.ShouldBe(expectedDesc);
            fetched.Website?.ToString().ShouldBe(expectedWebsite);
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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            brand.Update(newName, newDesc, newWebsite);
            _repository.Update(brand);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);

            // Assert
            fetched.ShouldNotBeNull();
            fetched!.Name.ShouldBe(newName ?? "PartialUpdateBrand");
            fetched.Description.ShouldBe(newDesc ?? "desc");
            fetched.Website?.ToString().ShouldBe(newWebsite ?? "https://partial.com");
        }

        [Fact]
        public async Task DeleteBrand_Works()
        {
            // Arrange
            var brandResult = Brand.Create("BrandToDelete", "desc", "https://brand.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            _repository.Delete(brand);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

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
            // Ensure clean state
            DbContext.Brands.RemoveRange(DbContext.Brands);
            await DbContext.SaveChangesAsync();

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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

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
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            await _repository.DeleteBrandsAsync(ids, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddBrand_WithInvalidName_Throws(string? name)
        {
            // Arrange
            var brandResult = Brand.Create(name, "desc", "https://valid.com");
            brandResult.IsError.ShouldBeTrue();
            var errorCodes = string.Join(", ", brandResult.Errors.Select(e => e.Code));
            brandResult.Errors.ShouldContain(e => e.Code == BrandErrors.EmptyName.Code, $"Actual error codes: {errorCodes}");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddBrand_WithInvalidDescription_Throws(string? desc)
        {
            // Arrange
            var brandResult = Brand.Create("ValidName", desc, "https://valid.com");
            brandResult.IsError.ShouldBeTrue();
            var errorCodes = string.Join(", ", brandResult.Errors.Select(e => e.Code));
            brandResult.Errors.ShouldContain(e => e.Code == BrandErrors.EmptyDescription.Code, $"Actual error codes: {errorCodes}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-url")]
        [InlineData("ftp://invalid.com")]
        [InlineData("http:/missing-slash.com")]
        [InlineData("www.noprotocol.com")]
        public async Task AddBrand_WithInvalidWebsite_Throws(string? website)
        {
            // Arrange
            var brandResult = Brand.Create("ValidName", "desc", website);
            brandResult.IsError.ShouldBeTrue();
            var errorCodes = string.Join(", ", brandResult.Errors.Select(e => e.Code));
            brandResult.Errors.ShouldContain(e => e.Code == BrandErrors.EmptyWebsite.Code || e.Code == WebsiteErrors.Invalid.Code, $"Actual error codes: {errorCodes}");
        }

        [Fact]
        public async Task UpdateBrand_WithInvalidName_Throws()
        {
            // Arrange
            var brandResult = Brand.Create("ValidName", "desc", "https://valid.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            var updateResult = brand.Update("   ", null, null);
            updateResult.IsError.ShouldBeTrue();
            var errorCodes = string.Join(", ", updateResult.Errors.Select(e => e.Code));
            updateResult.Errors.ShouldContain(e => e.Code == BrandErrors.EmptyName.Code, $"Actual error codes: {errorCodes}");
        }

        [Fact]
        public async Task UpdateBrand_WithInvalidDescription_Throws()
        {
            // Arrange
            var brandResult = Brand.Create("ValidName", "desc", "https://valid.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            var updateResult = brand.Update(null, "   ", null);
            updateResult.IsError.ShouldBeTrue();
            var errorCodes = string.Join(", ", updateResult.Errors.Select(e => e.Code));
            updateResult.Errors.ShouldContain(e => e.Code == BrandErrors.EmptyDescription.Code, $"Actual error codes: {errorCodes}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-url")]
        [InlineData("ftp://invalid.com")]
        [InlineData("http:/missing-slash.com")]
        [InlineData("www.noprotocol.com")]
        public async Task UpdateBrand_WithInvalidWebsite_Throws(string? website)
        {
            // Arrange
            var brandResult = Brand.Create("ValidName", "desc", "https://valid.com");
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            var updateResult = brand.Update(null, null, website);
            updateResult.IsError.ShouldBeTrue();
            var errorCodes = string.Join(", ", updateResult.Errors.Select(e => e.Code));
            updateResult.Errors.ShouldContain(e => e.Code == BrandErrors.EmptyWebsite.Code || e.Code == WebsiteErrors.Invalid.Code, $"Actual error codes: {errorCodes}");
        }

        [Theory]
        [InlineData(null, "desc", "https://valid.com", "Brand.EmptyName")]
        [InlineData("", "desc", "https://valid.com", "Brand.EmptyName")]
        [InlineData("   ", "desc", "https://valid.com", "Brand.EmptyName")]
        [InlineData("ValidName", null, "https://valid.com", "Brand.EmptyDescription")]
        [InlineData("ValidName", "", "https://valid.com", "Brand.EmptyDescription")]
        [InlineData("ValidName", "   ", "https://valid.com", "Brand.EmptyDescription")]
        [InlineData("ValidName", "desc", "", "Brand.EmptyWebsite")]
        [InlineData("ValidName", "desc", "   ", "Brand.EmptyWebsite")]
        [InlineData("ValidName", "desc", "invalid-url", "Website.Invalid")]
        [InlineData("ValidName", "desc", "ftp://notallowed.com", "Website.Invalid")]
        public async Task AddBrand_InvalidData_ReturnsError_AndNotPersisted(string? name, string? desc, string? website, string expectedErrorCode)
        {
            // Arrange
            var result = Brand.Create(name!, desc, website);

            // Assert error
            result.IsError.ShouldBeTrue();
            var errorCodes = string.Join(", ", result.Errors.Select(e => e.Code));
            result.Errors.ShouldContain(e => e.Code == expectedErrorCode, $"Actual error codes: {errorCodes}");

            // Try to persist if not error (should not happen)
            if (!result.IsError)
            {
                var brand = result.Value;
                await _repository.AddAsync(brand, CancellationToken.None);
                await UnitOfWork.SaveChangesAsync(CancellationToken.None);
                var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);
                fetched.ShouldBeNull();
            }
        }

        [Theory]
        [InlineData("ValidName", "desc", "https://valid.com", "", "Brand.EmptyName")]
        [InlineData("ValidName", "desc", "https://valid.com", "   ", "Brand.EmptyName")]
        [InlineData("ValidName", "desc", "https://valid.com", "", "Brand.EmptyDescription", true)]
        [InlineData("ValidName", "desc", "https://valid.com", "   ", "Brand.EmptyDescription", true)]
        [InlineData("ValidName", "desc", "https://valid.com", "invalid-url", "Website.Invalid", false, true)]
        [InlineData("ValidName", "desc", "https://valid.com", "ftp://notallowed.com", "Website.Invalid", false, true)]
        public async Task UpdateBrand_InvalidData_ReturnsError_AndNotPersisted(string initialName, string initialDesc, string initialWebsite, string? updateValue, string expectedErrorCode, bool updateDesc = false, bool updateWebsite = false)
        {
            // Arrange
            var brandResult = Brand.Create(initialName, initialDesc, initialWebsite);
            var brand = brandResult.Value;
            await _repository.AddAsync(brand, CancellationToken.None);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Act
            ErrorOr<Updated> updateResult;
            if (updateDesc)
                updateResult = brand.Update(null, updateValue, null);
            else if (updateWebsite)
                updateResult = brand.Update(null, null, updateValue);
            else
                updateResult = brand.Update(updateValue, null, null);

            // Assert error
            updateResult.IsError.ShouldBeTrue();
            var errorCodes = string.Join(", ", updateResult.Errors.Select(e => e.Code));
            updateResult.Errors.ShouldContain(e => e.Code == expectedErrorCode, $"Actual error codes: {errorCodes}");

            // Ensure not persisted
            _repository.Update(brand);
            await UnitOfWork.SaveChangesAsync(CancellationToken.None);
            var fetched = await _repository.FindByIdAsync(brand.Id, true, CancellationToken.None);
            fetched.ShouldNotBeNull();
            fetched.Name.ShouldBe(initialName);
            fetched.Description.ShouldBe(initialDesc);
            fetched.Website?.ToString().ShouldBe(initialWebsite);
        }
    }
}
