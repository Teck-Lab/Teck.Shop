using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.AutoNSubstitute;
using AutoFixture;
using Catalog.Application.Features.Brands.CreateBrand;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.UnitTests.Application.Brands;
using ErrorOr;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus;
using Catalog.Application.Features.Brands.UpdateBrand;
using Catalog.Application.Contracts.Caching;
using Catalog.Application.Contracts.Repositories;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Soenneker.Utils.AutoBogus.Override;
using Soenneker.Utils.AutoBogus.Context;
using Shouldly;

namespace Catalog.UnitTests.Application.Brands
{
    public class UpdateBrandCommandHandlerTests
    {
        //private readonly Substitute<IBrandRepository> _brandRepositoryMock;

        [Fact]
        public async Task Handle_Should_ReturnSuccessResult_WhenBrandIsUpdated_Async()
        {
            //Arrange
            IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });

            UpdateBrandCommandHandler sut = fixture.Create<UpdateBrandCommandHandler>();

            var optionalConfig = new AutoFakerConfig();
            var autoFaker = new AutoFaker(optionalConfig);
            autoFaker.Config.Overrides = [new UpdateBrandRequestOverride()];

            var request = autoFaker.Generate<UpdateBrandRequest>();

            UpdateBrandCommand command = new(request.Id, request.Name, request.Description, request.Website);

            //Act
            ErrorOr<BrandResponse> result = await sut.Handle(command, default);

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
            autoFaker.Config.Overrides = [new UpdateBrandRequestOverride()];


            var expected = autoFaker.Generate<BrandResponse>();

            fixture.Freeze<IBrandRepository>().FindByIdAsync(expected.Id, false, default).ReturnsNullForAnyArgs();

            UpdateBrandCommandHandler sut = fixture.Create<UpdateBrandCommandHandler>();

            var request = autoFaker.Generate<UpdateBrandRequest>();

            UpdateBrandCommand command = new(request.Id, request.Name, request.Description, request.Website);

            //Act
            ErrorOr<BrandResponse> result = await sut.Handle(command, default);

            //Assert
            result.IsError.ShouldBeTrue();
        }
    }

        public class UpdateBrandRequestOverride : AutoFakerOverride<UpdateBrandRequest>
    {
        public override void Generate(AutoFakerOverrideContext context)
        {
            var target = (context.Instance as UpdateBrandRequest)!;

            target.Name = context.Faker.Company.CompanyName();
            target.Description = context.Faker.Company.CatchPhrase();
            target.Website = context.Faker.Internet.Url();
        }
    }
}
