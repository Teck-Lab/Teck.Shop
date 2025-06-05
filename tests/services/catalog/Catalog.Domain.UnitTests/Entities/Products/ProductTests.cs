using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.ProductAggregate.Events;
using Shouldly;
using Xunit;

namespace Catalog.Domain.UnitTests.Entities.Products;

public class ProductTests
{
    private readonly IFixture _fixture;

    public ProductTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void Create_Should_SetCorrectProperties()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var sku = _fixture.Create<string>();
        var gtin = _fixture.Create<string>();
        var categories = _fixture.CreateMany<Category>().ToList();
        var brand = _fixture.Create<Brand>();

        // Act
        var result = Product.Create(
            name,
            description,
            sku,
            gtin,
            categories,
            isActive: true,
            brand);

        // Assert
        result.IsError.ShouldBeFalse();
        var product = result.Value;
        product.Name.ShouldBe(name);
        product.Description.ShouldBe(description);
        product.ProductSKU.ShouldBe(sku);
        product.GTIN.ShouldBe(gtin);
        product.Categories.ShouldBe(categories);
        product.IsActive.ShouldBeTrue();
        product.Brand.ShouldBe(brand);
        product.BrandId.ShouldBe(brand.Id);
        product.Slug.ShouldBe(name.ToLower().Replace(" ", "-"));
        product.GetDomainEvents().ShouldContain(e => e is ProductCreatedDomainEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_Should_ReturnError_When_NameIsInvalid(string invalidName)
    {
        // Arrange
        var description = _fixture.Create<string>();
        var sku = _fixture.Create<string>();
        var gtin = _fixture.Create<string>();
        var categories = _fixture.CreateMany<Category>().ToList();
        var brand = _fixture.Create<Brand>();

        // Act
        var result = Product.Create(
            invalidName,
            description,
            sku,
            gtin,
            categories,
            isActive: true,
            brand);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_DescriptionIsInvalid(string invalidDescription)
    {
        // Arrange
        var name = _fixture.Create<string>();
        var sku = _fixture.Create<string>();
        var gtin = _fixture.Create<string>();
        var categories = _fixture.CreateMany<Category>().ToList();
        var brand = _fixture.Create<Brand>();

        // Act
        var result = Product.Create(name, invalidDescription, sku, gtin, categories, true, brand);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("description");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_SKUIsInvalid(string invalidSku)
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var gtin = _fixture.Create<string>();
        var categories = _fixture.CreateMany<Category>().ToList();
        var brand = _fixture.Create<Brand>();

        // Act
        var result = Product.Create(name, description, invalidSku, gtin, categories, true, brand);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("SKU");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_GTINIsInvalid(string invalidGtin)
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var sku = _fixture.Create<string>();
        var categories = _fixture.CreateMany<Category>().ToList();
        var brand = _fixture.Create<Brand>();

        // Act
        var result = Product.Create(name, description, sku, invalidGtin, categories, true, brand);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("GTIN");
    }

    [Fact]
    public void Create_Should_ReturnError_When_CategoriesIsEmpty()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var sku = _fixture.Create<string>();
        var gtin = _fixture.Create<string>();
        var brand = _fixture.Create<Brand>();

        // Act
        var result = Product.Create(name, description, sku, gtin, new List<Category>(), true, brand);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("categories");
    }

    [Fact]
    public void Create_Should_ReturnError_When_BrandIsNull()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var sku = _fixture.Create<string>();
        var gtin = _fixture.Create<string>();
        var categories = _fixture.CreateMany<Category>().ToList();
        Brand brand = null;

        // Act
        var result = Product.Create(name, description, sku, gtin, categories, true, brand);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("brand");
    }

    [Fact]
    public void Create_Should_GenerateValidSlug()
    {
        // Arrange
        var name = "Product Name With Spaces!";
        var description = _fixture.Create<string>();
        var sku = _fixture.Create<string>();
        var gtin = _fixture.Create<string>();
        var categories = _fixture.CreateMany<Category>().ToList();
        var brand = _fixture.Create<Brand>();

        // Act
        var result = Product.Create(name, description, sku, gtin, categories, true, brand);

        // Assert
        result.IsError.ShouldBeFalse();
        var product = result.Value;
        product.Slug.ShouldBe("product-name-with-spaces");
    }

    [Fact]
    public void Update_Should_OnlyModifyChangedProperties()
    {
        // Arrange
        var createResult = Product.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.CreateMany<Category>().ToList(),
            true,
            _fixture.Create<Brand>()
        );
        var product = createResult.Value;
        var originalDescription = product.Description;
        var newName = _fixture.Create<string>();

        // Act
        var updateResult = product.Update(newName);

        // Assert
        updateResult.IsError.ShouldBeFalse();
        product.Name.ShouldBe(newName);
        product.Description.ShouldBe(originalDescription);
        product.Slug.ShouldBe(newName.ToLower().Replace(" ", "-"));
    }

    // Uncomment and adapt these if your Product aggregate supports AddCategory/RemoveCategory with ErrorOr
    /*
    [Fact]
    public void AddCategory_Should_AddToCategories()
    {
        // Arrange
        var createResult = Product.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            new List<Category>(),
            true,
            _fixture.Create<Brand>()
        );
        var product = createResult.Value;
        var category = _fixture.Create<Category>();

        // Act
        var addResult = product.AddCategory(category);

        // Assert
        addResult.IsError.ShouldBeFalse();
        product.Categories.ShouldContain(category);
    }

    [Fact]
    public void RemoveCategory_Should_RemoveFromCategories()
    {
        // Arrange
        var category = _fixture.Create<Category>();
        var createResult = Product.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            new List<Category> { category },
            true,
            _fixture.Create<Brand>()
        );
        var product = createResult.Value;

        // Act
        var removeResult = product.RemoveCategory(category);

        // Assert
        removeResult.IsError.ShouldBeFalse();
        product.Categories.ShouldNotContain(category);
    }
    */
}