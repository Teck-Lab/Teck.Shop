using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.AutoNSubstitute;
using AutoFixture;
using Catalog.Application.Contracts.Repositories;
using Catalog.Application.Features.Brands.DeleteBrand;
using ErrorOr;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus;
using Catalog.Application.Features.Brands.DeleteBrands;
using NSubstitute.ReturnsExtensions;
using Shouldly;

namespace Catalog.UnitTests.Application.Brands
{
    public class DeleteBrandsCommandHandlerTests
    {
        //private readonly Substitute<IBrandRepository> _brandRepositoryMock;

        [Fact]
        public async Task Handle_Should_ReturnDeletedResult_WhenBrandsIsDeleted_Async()
        {
            //Arrange
            IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });

            DeleteBrandsCommandHandler sut = fixture.Create<DeleteBrandsCommandHandler>();

            var optionalConfig = new AutoFakerConfig();
            var autoFaker = new AutoFaker(optionalConfig);

            var command = autoFaker.Generate<DeleteBrandsCommand>();

            //Act
            ErrorOr<Deleted> result = await sut.Handle(command, default);

            //Assert
            result.IsError.ShouldBeFalse();
        }
    }
}
