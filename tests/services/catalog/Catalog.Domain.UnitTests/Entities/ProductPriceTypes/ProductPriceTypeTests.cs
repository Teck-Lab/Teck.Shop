using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Catalog.Domain.Entities.ProductPriceTypeAggregate;
using Shouldly;
using Xunit;

namespace Catalog.Domain.UnitTests.Entities.ProductPriceTypes;

public class ProductPriceTypeTests
{
    private readonly IFixture _fixture;

    public ProductPriceTypeTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void Create_Should_SetCorrectProperties()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var priority = _fixture.Create<int>();

        // Act
        var result = ProductPriceType.Create(name, priority);

        // Assert
        result.IsError.ShouldBeFalse();
        var priceType = result.Value;
        priceType.Name.ShouldBe(name);
        priceType.Priority.ShouldBe(priority);
        priceType.ProductPrices.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_NameIsInvalid(string invalidName)
    {
        // Arrange
        var priority = _fixture.Create<int>();

        // Act
        var result = ProductPriceType.Create(invalidName, priority);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("name");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_Should_ReturnError_When_PriorityIsNegative(int invalidPriority)
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act
        var result = ProductPriceType.Create(name, invalidPriority);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("cannot be negative");
    }

    [Fact]
    public void Update_Should_ModifyProperties()
    {
        // Arrange
        var createResult = ProductPriceType.Create(
            _fixture.Create<string>(),
            1
        );
        var priceType = createResult.Value;
        var newName = _fixture.Create<string>();
        var newPriority = 2;

        // Act
        var updateResult = priceType.Update(newName, newPriority);

        // Assert
        updateResult.IsError.ShouldBeFalse();
        priceType.Name.ShouldBe(newName);
        priceType.Priority.ShouldBe(newPriority);
    }
}