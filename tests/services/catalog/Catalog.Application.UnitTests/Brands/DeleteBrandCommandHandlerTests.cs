using AutoFixture.AutoNSubstitute;
using AutoFixture;
using ErrorOr;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus;
using Catalog.Application.Features.Brands.DeleteBrand;
using Catalog.Application.Contracts.Repositories;
using NSubstitute.ReturnsExtensions;
using Shouldly;

namespace Catalog.Application.UnitTests.Brands
{
    public class DeleteBrandCommandHandlerTests
    {
        //private readonly Substitute<IBrandRepository> _brandRepositoryMock;

        [Fact]
        public async Task Handle_Should_ReturnDeletedResult_WhenBrandIsDeleted_Async()
        {
            //Arrange
            IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });

            DeleteBrandCommandHandler sut = fixture.Create<DeleteBrandCommandHandler>();

            var optionalConfig = new AutoFakerConfig();
            var autoFaker = new AutoFaker(optionalConfig);

            var command = autoFaker.Generate<DeleteBrandCommand>();

            //Act
            ErrorOr<Deleted> result = await sut.Handle(command, default);

            //Assert
            result.IsError.ShouldBeFalse();
        }
        [Fact]
        public async Task Handle_Should_ReturnNotFoundResult_WhenBrandIsNotFound_Async()
        {
            //Arrange
            IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });

            var optionalConfig = new AutoFakerConfig();
            var autoFaker = new AutoFaker(optionalConfig);

            var command = autoFaker.Generate<DeleteBrandCommand>();

            var repo = fixture.Freeze<IBrandRepository>().FindOneAsync(brand => brand.Id.Equals(Guid.NewGuid()), true, default).ReturnsNullForAnyArgs();

            DeleteBrandCommandHandler sut = fixture.Create<DeleteBrandCommandHandler>();

            //Act
            ErrorOr<Deleted> result = await sut.Handle(command, default);

            //Assert
            result.IsError.ShouldBeTrue();
        }
    }
}
