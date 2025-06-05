using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.PromotionAggregate;
using Shouldly;
using Xunit;

namespace Catalog.Domain.UnitTests.Entities.Promotions;

public class PromotionTests
{
    private readonly IFixture _fixture;

    public PromotionTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnError_When_NameIsInvalid(string invalidName)
    {
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddDays(1);
        var products = new List<Product>();
        var result = Promotion.Create(invalidName, null, start, end, products);
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("name");
    }

    [Fact]
    public void Create_Should_ReturnError_When_StartDateAfterEndDate()
    {
        var name = _fixture.Create<string>();
        var start = DateTime.UtcNow.AddDays(2);
        var end = DateTime.UtcNow.AddDays(1);
        var products = new List<Product>();
        var result = Promotion.Create(name, null, start, end, products);
        result.IsError.ShouldBeTrue();
        result.FirstError.Description.ShouldContain("start date");
    }

    [Fact]
    public void Update_Should_OnlyModifyChangedProperties()
    {
        // Arrange
        var createResult = Promotion.Create(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            _fixture.CreateMany<Product>().ToList()
        );
        var promotion = createResult.Value;
        var originalName = promotion.Name;
        var newDescription = _fixture.Create<string>();
        var newValidTo = promotion.ValidTo.AddDays(30);

        // Act
        var updateResult = promotion.Update(null, newDescription, null, newValidTo, null);

        // Assert
        updateResult.IsError.ShouldBeFalse();
        promotion.Name.ShouldBe(originalName);
        promotion.Description.ShouldBe(newDescription);
        promotion.ValidTo.ShouldBe(newValidTo);
    }

    // You can similarly update and uncomment the following tests if your Promotion entity supports ErrorOr and these methods.
}