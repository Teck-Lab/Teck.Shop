using AutoFixture;
using Catalog.Domain.Entities.SupplierAggregate;
using Shouldly;
using Xunit;
using AutoFixture.AutoNSubstitute;

namespace Catalog.Domain.UnitTests.Entities.Suppliers;

public class SupplierTests
{
    private readonly IFixture _fixture;

    public SupplierTests()
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
        var result = Supplier.Create(name, description, website);

        // Assert
        result.IsError.ShouldBeFalse();
        var supplier = result.Value;
        supplier.Name.ShouldBe(name);
        supplier.Description.ShouldBe(description);
        supplier.Website.ShouldBe(website);
    }

    [Fact]
    public void Update_Should_OnlyModifyChangedProperties()
    {
        // Arrange
        var createResult = Supplier.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            "https://example.com"
        );
        var supplier = createResult.Value;
        var originalName = supplier.Name;
        var newDescription = _fixture.Create<string>();
        var newWebsite = "https://newsite.com";

        // Act
        var updateResult = supplier.Update(null, newDescription, newWebsite);

        // Assert
        updateResult.IsError.ShouldBeFalse();
        supplier.Name.ShouldBe(originalName);
        supplier.Description.ShouldBe(newDescription);
        supplier.Website.ShouldBe(newWebsite);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_NameIsInvalid(string invalidName)
    {
        // Arrange
        var website = "https://example.com";

        // Act
        var result = Supplier.Create(invalidName, website, null);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_WebsiteIsInvalid(string invalidWebsite)
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act
        var result = Supplier.Create(name, invalidWebsite, null);

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

        // Act
        var result = Supplier.Create(name, null, badWebsite);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Supplier.InvalidWebsite");
    }
}