using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.BrandAggregate.Events;
using Shouldly;
using Xunit;

namespace Catalog.UnitTests.Domain.Brands;

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
    public void Create_Should_Succeed_When_WebsiteIsNull()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        string website = null;

        // Act
        var result = Brand.Create(name, description, website);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Website.ShouldBeNull();
    }

    [Fact]
    public void Create_Should_ReturnMultipleErrors_When_MultipleFieldsInvalid()
    {
        // Arrange
        string name = null;
        string description = " ";
        string website = "bad-url";

        // Act
        var result = Brand.Create(name, description, website);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void Create_Should_Succeed_With_Uppercase_Http_Or_Https()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var website = "HTTPS://EXAMPLE.COM";

        // Act
        var result = Brand.Create(name, description, website);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Website.ShouldBe(website);
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

    [Fact]
    public void Update_Should_Succeed_When_AllFieldsNull()
    {
        // Arrange
        var createResult = Brand.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            "https://example.com"
        );
        var brand = createResult.Value;
        var originalName = brand.Name;
        var originalDescription = brand.Description;
        var originalWebsite = brand.Website;

        // Act
        var updateResult = brand.Update(null, null, null);

        // Assert
        updateResult.IsError.ShouldBeFalse();
        brand.Name.ShouldBe(originalName);
        brand.Description.ShouldBe(originalDescription);
        brand.Website.ShouldBe(originalWebsite);
    }

    [Fact]
    public void Update_Should_ReturnMultipleErrors_When_MultipleFieldsInvalid()
    {
        // Arrange
        var createResult = Brand.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            "https://example.com"
        );
        var brand = createResult.Value;

        // Act
        var updateResult = brand.Update("", "", "bad-url");

        // Assert
        updateResult.IsError.ShouldBeTrue();
        updateResult.Errors.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void Update_Should_NotChange_When_ValuesAreSame()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var website = "https://example.com";
        var createResult = Brand.Create(name, description, website);
        var brand = createResult.Value;

        // Act
        var updateResult = brand.Update(name, description, website);

        // Assert
        updateResult.IsError.ShouldBeFalse();
        brand.Name.ShouldBe(name);
        brand.Description.ShouldBe(description);
        brand.Website.ShouldBe(website);
    }

    [Fact]
    public void Update_Should_ReturnError_When_DescriptionIsEmpty()
    {
        var createResult = Brand.Create(_fixture.Create<string>(), _fixture.Create<string>(), "https://example.com");
        var brand = createResult.Value;
        var updateResult = brand.Update(null, "", null);
        updateResult.IsError.ShouldBeTrue();
        updateResult.Errors.ShouldContain(e => e.Description.Contains("description"));
    }

    [Fact]
    public void Update_Should_ReturnError_When_WebsiteIsEmpty()
    {
        var createResult = Brand.Create(_fixture.Create<string>(), _fixture.Create<string>(), "https://example.com");
        var brand = createResult.Value;
        var updateResult = brand.Update(null, null, "");
        updateResult.IsError.ShouldBeTrue();
        updateResult.Errors.ShouldContain(e => e.Description.Contains("website"));
    }

    [Fact]
    public void Update_Should_SetWebsite_When_PreviouslyNull()
    {
        var createResult = Brand.Create(_fixture.Create<string>(), _fixture.Create<string>(), null);
        var brand = createResult.Value;
        var newWebsite = "https://newsite.com";
        var updateResult = brand.Update(null, null, newWebsite);
        updateResult.IsError.ShouldBeFalse();
        brand.Website.ShouldBe(newWebsite);
    }

    [Fact]
    public void Update_Should_NotChange_When_DescriptionIsSame()
    {
        var description = _fixture.Create<string>();
        var createResult = Brand.Create(_fixture.Create<string>(), description, "https://example.com");
        var brand = createResult.Value;
        var updateResult = brand.Update(null, description, null);
        updateResult.IsError.ShouldBeFalse();
        brand.Description.ShouldBe(description);
    }

    [Fact]
    public void Update_Should_NotChange_When_WebsiteIsSame()
    {
        var website = "https://example.com";
        var createResult = Brand.Create(_fixture.Create<string>(), _fixture.Create<string>(), website);
        var brand = createResult.Value;
        var updateResult = brand.Update(null, null, website);
        updateResult.IsError.ShouldBeFalse();
        brand.Website.ShouldBe(website);
    }

    [Fact]
    public void Products_Should_Initialize_As_EmptyCollection()
    {
        var createResult = Brand.Create(_fixture.Create<string>(), _fixture.Create<string>(), "https://example.com");
        var brand = createResult.Value;
        brand.Products.ShouldNotBeNull();
        brand.Products.ShouldBeEmpty();
    }

    [Fact]
    public void Create_Should_ReturnError_When_WebsiteIsEmptyString()
    {
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var website = "";
        var result = Brand.Create(name, description, website);
        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Description.Contains("website"));
    }

    [Fact]
    public void Update_Should_UpdateAllFields_When_AllProvided()
    {
        var createResult = Brand.Create(_fixture.Create<string>(), _fixture.Create<string>(), "https://example.com");
        var brand = createResult.Value;
        var newName = _fixture.Create<string>();
        var newDescription = _fixture.Create<string>();
        var newWebsite = "https://newsite.com";
        var updateResult = brand.Update(newName, newDescription, newWebsite);
        updateResult.IsError.ShouldBeFalse();
        brand.Name.ShouldBe(newName);
        brand.Description.ShouldBe(newDescription);
        brand.Website.ShouldBe(newWebsite);
    }

    [Fact]
    public void Update_Should_ReturnError_When_WebsiteIsEmptyString()
    {
        var createResult = Brand.Create(_fixture.Create<string>(), _fixture.Create<string>(), "https://example.com");
        var brand = createResult.Value;
        var updateResult = brand.Update(null, null, "");
        updateResult.IsError.ShouldBeTrue();
        updateResult.Errors.ShouldContain(e => e.Description.Contains("website"));
    }
}