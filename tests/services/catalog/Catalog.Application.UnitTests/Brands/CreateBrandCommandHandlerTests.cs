using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Catalog.Application.Features.Brands.CreateBrand.V1;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Contracts.Repositories;
using Catalog.Application.Contracts.Caching;
using ErrorOr;
using Shouldly;
using Soenneker.Utils.AutoBogus;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus.Context;
using Soenneker.Utils.AutoBogus.Override;
using Teck.Shop.SharedKernel.Core.Database;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Catalog.Domain.Entities.BrandAggregate;

namespace Catalog.Application.UnitTests.Brands
{
    public class CreateBrandCommandHandlerTests
    {
        //private readonly Substitute<IBrandRepository> _brandRepositoryMock;

        [Fact]
        public async Task Handle_Should_ReturnSuccessResult_WhenBrandIsUnique_Async()
        {
            //Arrange
            IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true});

            CreateBrandCommandHandler sut = fixture.Create<CreateBrandCommandHandler>();

            var optionalConfig = new AutoFakerConfig();
            var autoFaker = new AutoFaker(optionalConfig);
            autoFaker.Config.Overrides = [new CreateBrandRequestOverride()];

            var request = autoFaker.Generate<CreateBrandRequest>();

            CreateBrandCommand command = new(request.Name, request.Description, request.Website);

            //Act
            ErrorOr<BrandResponse> result = await sut.Handle(command, default);

            //Assert
            result.IsError.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_Should_ReturnError_WhenBrandIsInvalid_Async()
        {
            // Arrange
            IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });
            var sut = fixture.Create<CreateBrandCommandHandler>();
            var command = new CreateBrandCommand("", null, "not-a-url");
            // Act
            var result = await sut.Handle(command, default);
            // Assert
            result.IsError.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_Should_Throw_WhenRepositoryThrows_Async()
        {
            // Arrange
            var repo = Substitute.For<IBrandRepository>();
            var uow = Substitute.For<IUnitOfWork>();
            var cache = Substitute.For<IBrandCache>();
            repo.AddAsync(Arg.Any<Brand>(), Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("db error"));
            var sut = new CreateBrandCommandHandler(uow, cache, repo);
            var command = new CreateBrandCommand("ValidName", "Valid description", "https://example.com");
            // Act & Assert
            await Should.ThrowAsync<Exception>(async () => await sut.Handle(command, default));
        }

        [Fact]
        public async Task Handle_Should_Throw_WhenUnitOfWorkThrows_Async()
        {
            // Arrange
            var repo = Substitute.For<IBrandRepository>();
            var uow = Substitute.For<IUnitOfWork>();
            var cache = Substitute.For<IBrandCache>();
            uow.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("uow error"));
            var sut = new CreateBrandCommandHandler(uow, cache, repo);
            var command = new CreateBrandCommand("ValidName", "Valid description", "https://example.com");
            // Act & Assert
            await Should.ThrowAsync<Exception>(async () => await sut.Handle(command, default));
        }

        [Fact]
        public async Task Handle_Should_Throw_WhenCacheThrows_Async()
        {
            // Arrange
            var repo = Substitute.For<IBrandRepository>();
            var uow = Substitute.For<IUnitOfWork>();
            var cache = Substitute.For<IBrandCache>();
            cache.SetAsync(Arg.Any<Guid>(), Arg.Any<Brand>(), Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("cache error"));
            var sut = new CreateBrandCommandHandler(uow, cache, repo);
            var command = new CreateBrandCommand("ValidName", "Valid description", "https://example.com");
            // Act & Assert
            await Should.ThrowAsync<Exception>(async () => await sut.Handle(command, default));
        }
    }

    public class CreateBrandRequestOverride : AutoFakerOverride<CreateBrandRequest>
    {
        public override void Generate(AutoFakerOverrideContext context)
        {
            var target = (context.Instance as CreateBrandRequest)!;

            target.Name = context.Faker.Company.CompanyName();
            target.Description = context.Faker.Company.CatchPhrase();
            target.Website = context.Faker.Internet.Url();
        }
    }
}