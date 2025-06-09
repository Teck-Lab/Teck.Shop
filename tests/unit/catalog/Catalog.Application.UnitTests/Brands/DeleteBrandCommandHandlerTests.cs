using AutoFixture.AutoNSubstitute;
using AutoFixture;
using ErrorOr;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus;
using Catalog.Application.Features.Brands.DeleteBrand;
using Catalog.Application.Contracts.Repositories;
using NSubstitute.ReturnsExtensions;
using Shouldly;
using NSubstitute;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Application.Contracts.Caching;
using Teck.Shop.SharedKernel.Core.Database;
using System.Linq.Expressions;

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

        [Fact]
        public async Task Handle_Should_Throw_WhenRepositoryThrows_Async()
        {
            var repo = Substitute.For<IBrandRepository>();
            var uow = Substitute.For<IUnitOfWork>();
            var cache = Substitute.For<IBrandCache>();
#pragma warning disable CS8603 // Possible null reference return.
            repo.FindOneAsync(Arg.Any<Expression<Func<Brand, bool>>>(), true, Arg.Any<CancellationToken>()).Returns(call => Task.FromException<Brand>(new Exception("repo error")));
#pragma warning restore CS8603
            var sut = new DeleteBrandCommandHandler(uow, cache, repo);
            var command = new DeleteBrandCommand(Guid.NewGuid());
            await Should.ThrowAsync<Exception>(async () => await sut.Handle(command, default));
        }

        [Fact]
        public async Task Handle_Should_Throw_WhenUnitOfWorkThrows_Async()
        {
            var repo = Substitute.For<IBrandRepository>();
            var uow = Substitute.For<IUnitOfWork>();
            var cache = Substitute.For<IBrandCache>();
            repo.FindOneAsync(Arg.Any<Expression<Func<Brand, bool>>>(), true, Arg.Any<CancellationToken>()).Returns(new Brand());
            uow.When(x => x.SaveChangesAsync(Arg.Any<CancellationToken>())).Do(x => throw new Exception("uow error"));
            var sut = new DeleteBrandCommandHandler(uow, cache, repo);
            var command = new DeleteBrandCommand(Guid.NewGuid());
            await Should.ThrowAsync<Exception>(async () => await sut.Handle(command, default));
        }

        [Fact]
        public async Task Handle_Should_Throw_WhenCacheThrows_Async()
        {
            var repo = Substitute.For<IBrandRepository>();
            var uow = Substitute.For<IUnitOfWork>();
            var cache = Substitute.For<IBrandCache>();
            repo.FindOneAsync(Arg.Any<Expression<Func<Brand, bool>>>(), true, Arg.Any<CancellationToken>()).Returns(new Brand());
            uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
            cache.When(x => x.RemoveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())).Do(x => throw new Exception("cache error"));
            var sut = new DeleteBrandCommandHandler(uow, cache, repo);
            var command = new DeleteBrandCommand(Guid.NewGuid());
            await Should.ThrowAsync<Exception>(async () => await sut.Handle(command, default));
        }
    }
}
