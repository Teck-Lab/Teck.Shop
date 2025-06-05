using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Catalog.Domain.Entities.CategoryAggregate;
using Shouldly;
using Xunit;

namespace Catalog.Domain.UnitTests.Entities.Categories;

public class CategoryTests
{
    private readonly IFixture _fixture;

    public CategoryTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void Create_Should_SetCorrectProperties()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();

        // Act
        var result = Category.Create(name, description);

        // Assert
        result.IsError.ShouldBeFalse();
        var category = result.Value;
        category.Name.ShouldBe(name);
        category.Description.ShouldBe(description);
        category.Products.ShouldBeEmpty();
        category.Promotions.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_Should_ReturnError_When_NameIsInvalid(string invalidName)
    {
        // Arrange
        var description = _fixture.Create<string>();

        // Act
        var result = Category.Create(invalidName, description);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_Should_ReturnError_When_DescriptionIsInvalid(string invalidDescription)
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act
        var result = Category.Create(name, invalidDescription);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("description");
    }

    [Fact]
    public void Update_Should_OnlyModifyChangedProperties()
    {
        // Arrange
        var createResult = Category.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var category = createResult.Value;
        var originalName = category.Name;
        var newDescription = _fixture.Create<string>();

        // Act
        var updateResult = category.Update(null, newDescription);

        // Assert
        updateResult.IsError.ShouldBeFalse();
        category.Name.ShouldBe(originalName);
        category.Description.ShouldBe(newDescription);
    }
}