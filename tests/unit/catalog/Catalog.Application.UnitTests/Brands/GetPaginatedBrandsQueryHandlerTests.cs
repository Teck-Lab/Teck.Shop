using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.AutoNSubstitute;
using AutoFixture;
using Catalog.Application.Contracts.Caching;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.GetBrand;
using ErrorOr;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus;
using Catalog.Application.Features.Brands.GetPaginatedBrands;
using Teck.Shop.SharedKernel.Core.Pagination;
using Catalog.Application.Contracts.Repositories;
using NSubstitute;
using Catalog.Domain.Entities.BrandAggregate;
using Shouldly;

namespace Catalog.Application.UnitTests.Brands
{
    public class GetPaginatedBrandsQueryHandlerTests
    {
        //private readonly Substitute<IBrandRepository> _brandRepositoryMock;

        [Fact]
        public async Task Handle_Should_ReturnEmptyBrandResponseList_WhenBrandsIsFound_Async()
        {
            //Arrange
            IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });

            var optionalConfig = new AutoFakerConfig();
            var autoFaker = new AutoFaker(optionalConfig);

            //var expected = autoFaker.Generate<PagedList<BrandResponse>>();
            var expected = new PagedList<Brand>([], 0, 1, 10);
            var request = autoFaker.Generate<GetPaginatedBrandsQuery>();

            fixture.Freeze<IBrandRepository>().GetPagedBrandsAsync(request.Page, request.Size, request.Keyword, default).Returns(expected);

            GetPaginatedBrandsQueryHandler sut = fixture.Create<GetPaginatedBrandsQueryHandler>();

            //Act
            PagedList<BrandResponse> result = await sut.Handle(request, default);

            //Assert
            result.TotalItems.ShouldBe(0);
        }
    }
}
