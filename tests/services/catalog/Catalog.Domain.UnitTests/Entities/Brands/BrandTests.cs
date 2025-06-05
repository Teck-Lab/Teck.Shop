using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.BrandAggregate.Events;
using Shouldly;
using Xunit;

namespace Catalog.Domain.UnitTests.Entities.Brands;

public class BrandTests
{
    private readonly IFixture _fixture;

    public BrandTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void Create_Should_SetCorrectProperties()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var website = "https://example.com"; // Use a valid absolute URL

        // Act
        var result = Brand.Create(name, description, website);

        // Assert
        result.IsError.ShouldBeFalse();
        var brand = result.Value;
        brand.Name.ShouldBe(name);
        brand.Description.ShouldBe(description);
        brand.Website.ShouldBe(website);
        brand.Products.ShouldBeEmpty();
        brand.GetDomainEvents().Count(e => e is BrandCreatedDomainEvent).ShouldBe(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_NameIsInvalid(string invalidName)
    {
        // Arrange
        var description = _fixture.Create<string>();
        var website = "https://example.com";

        // Act
        var result = Brand.Create(invalidName, description, website);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_DescriptionIsInvalid(string invalidDescription)
    {
        // Arrange
        var name = _fixture.Create<string>();
        var website = "https://example.com";

        // Act
        var result = Brand.Create(name, invalidDescription, website);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("description");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_WebsiteIsInvalid(string invalidWebsite)
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();

        // Act
        var result = Brand.Create(name, description, invalidWebsite);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("website");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://badurl")]
    [InlineData("http:/missing-slash.com")]
    public void Create_Should_ReturnError_When_WebsiteIsMalformed(string badWebsite)
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();

        // Act
        var result = Brand.Create(name, description, badWebsite);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("valid URL");
    }

    [Fact]
    public void Update_Should_OnlyModifyChangedProperties()
    {
        // Arrange
        var createResult = Brand.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            "https://example.com" // Use a valid absolute URL
        );
        var brand = createResult.Value;
        var originalName = brand.Name;
        var newDescription = _fixture.Create<string>();
        var newWebsite = "https://newsite.com"; // Use a valid absolute URL

        // Act
        var updateResult = brand.Update(null, newDescription, newWebsite);

        // Assert
        updateResult.IsError.ShouldBeFalse();
        brand.Name.ShouldBe(originalName);
        brand.Description.ShouldBe(newDescription);
        brand.Website.ShouldBe(newWebsite);
    }

    [Fact]
    public void Update_Should_ReturnError_When_WebsiteIsInvalid()
    {
        // Arrange
        var createResult = Brand.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            "https://example.com"
        );
        var brand = createResult.Value;
        var invalidUrl = "not-a-valid-url";

        // Act
        var updateResult = brand.Update(null, null, invalidUrl);

        // Assert
        updateResult.IsError.ShouldBeTrue();
        updateResult.FirstError.Description.ShouldContain("valid URL");
    }

    [Fact]
    public void Website_Should_ReturnError_When_InvalidUrl()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var invalidUrl = "not-a-valid-url";

        // Act
        var result = Brand.Create(name, description, invalidUrl);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("valid URL");
    }
}