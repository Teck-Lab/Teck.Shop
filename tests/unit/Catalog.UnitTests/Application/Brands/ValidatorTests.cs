using System;
using System.Collections.Generic;
using Catalog.Application.Features.Brands.DeleteBrand;
using Catalog.Application.Features.Brands.DeleteBrands;
using Catalog.Application.Features.Brands.GetBrand;
using Catalog.Application.Features.Brands.GetPaginatedBrands;
using Catalog.Application.Features.Brands.UpdateBrand;
using Xunit;
using FastEndpoints;
using NSubstitute;
using FastEndpoints.Testing;
using Catalog.Application.Contracts.Repositories;
using Catalog.Application.Features.Brands.CreateBrand.V1;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Catalog.UnitTests.Application.Brands
{
    public class ValidatorTests
    {
        [Fact]
        public void DeleteBrandValidator_Should_Fail_On_Empty_Id()
        {
            var validator = new DeleteBrandValidator();
            var result = validator.Validate(new DeleteBrandRequest { Id = Guid.Empty });
            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void DeleteBrandsValidator_Should_Fail_On_Empty_Id()
        {
            var validator = new DeleteBrandsValidator();
            var result = validator.Validate(new DeleteBrandRequest { Id = Guid.Empty });
            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void GetBrandValidator_Should_Fail_On_Empty_Id()
        {
            var validator = new GetBrandValidator();
            var result = validator.Validate(new GetBrandRequest { Id = Guid.Empty });
            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void GetPaginatedBrandsValidator_Should_Fail_On_Zero_Page_Or_Size()
        {
            var validator = new GetPaginatedBrandsValidator();
            var req = new GetPaginatedBrandsRequest { Page = 0, Size = 0 };
            var result = validator.Validate(req);
            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void UpdateBrandValidator_Should_Fail_On_Empty_Id_Or_Name()
        {
            var validator = new UpdateBrandValidator();
            var req = new UpdateBrandRequest { Id = Guid.Empty, Name = "" };
            var result = validator.Validate(req);
            result.IsValid.ShouldBeFalse();
        }

        [Fact]
        public async Task CreateBrandValidator_Should_Fail_On_Empty_Name()
        {
            var repo = Substitute.For<IBrandRepository>();
            var validator = Factory.CreateValidator<CreateBrandValidator>(s => s.AddSingleton(repo));
            var req = new CreateBrandRequest { Name = "" };
            var result = await validator.ValidateAsync(req, TestContext.Current.CancellationToken);
            result.IsValid.ShouldBe(false);
        }

        [Fact]
        public async Task CreateBrandValidator_Should_Fail_When_Name_Already_Exists()
        {
            var repo = Substitute.For<IBrandRepository>();
            repo.ExistsAsync(
                NSubstitute.Arg.Any<System.Linq.Expressions.Expression<System.Func<Catalog.Domain.Entities.BrandAggregate.Brand, bool>>>(),
                NSubstitute.Arg.Any<bool>(),
                NSubstitute.Arg.Any<System.Threading.CancellationToken>())
                .Returns(Task.FromResult(true));
            var validator = Factory.CreateValidator<CreateBrandValidator>(s => s.AddSingleton(repo));
            var req = new CreateBrandRequest { Name = "DUPLICATE" };
            var result = await validator.ValidateAsync(req, TestContext.Current.CancellationToken);
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.ErrorMessage.Contains("already Exists"));
        }

        [Fact]
        public void DeleteBrandValidator_Should_Pass_On_Valid_Id()
        {
            var validator = new DeleteBrandValidator();
            var result = validator.Validate(new DeleteBrandRequest { Id = Guid.NewGuid() });
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void DeleteBrandsValidator_Should_Pass_On_Valid_Id()
        {
            var validator = new DeleteBrandsValidator();
            var result = validator.Validate(new DeleteBrandRequest { Id = Guid.NewGuid() });
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void GetBrandValidator_Should_Pass_On_Valid_Id()
        {
            var validator = new GetBrandValidator();
            var result = validator.Validate(new GetBrandRequest { Id = Guid.NewGuid() });
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void GetPaginatedBrandsValidator_Should_Pass_On_Valid_Page_And_Size()
        {
            var validator = new GetPaginatedBrandsValidator();
            var req = new GetPaginatedBrandsRequest { Page = 1, Size = 10 };
            var result = validator.Validate(req);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void UpdateBrandValidator_Should_Pass_On_Valid_Id_And_Name()
        {
            var validator = new UpdateBrandValidator();
            var req = new UpdateBrandRequest { Id = Guid.NewGuid(), Name = "Valid Brand" };
            var result = validator.Validate(req);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void UpdateBrandValidator_Should_Fail_On_Too_Long_Name()
        {
            var validator = new UpdateBrandValidator();
            var req = new UpdateBrandRequest { Id = Guid.NewGuid(), Name = new string('a', 101) };
            var result = validator.Validate(req);
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Name");
        }
    }
}
