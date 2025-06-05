using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Catalog.Application.Features.Brands.CreateBrand.V1;
using Catalog.Application.Features.Brands.Dtos;
using ErrorOr;
using Shouldly;
using Soenneker.Utils.AutoBogus;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus.Context;
using Soenneker.Utils.AutoBogus.Override;

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