using System;
using System.Collections.Generic;
using Catalog.Application.Contracts.Repositories;
using Catalog.Application.Features.Products.CreateProduct.V1;
using Catalog.Application.Features.Products.GetProductById.V1;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Catalog.Application.UnitTests.Products
{
    public class ValidatorTests
    {
        [Fact]
        public async Task CreateProductValidator_Should_Fail_On_Empty_ProductSku()
        {
            var repo = Substitute.For<IProductRepository>();
            var validator = Factory.CreateValidator<CreateProductValidator>(s => s.AddSingleton(repo));
            var req = new CreateProductRequest { ProductSku = "" };
            var result = await validator.ValidateAsync(req, TestContext.Current.CancellationToken);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void GetProductByIdValidator_Should_Fail_On_Empty_ProductId()
        {
            var validator = new GetProductByIdValidator();
            var req = new GetProductByIdRequest { ProductId = Guid.Empty };
            var result = validator.Validate(req);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void GetProductByIdValidator_Should_Pass_On_Valid_ProductId()
        {
            var validator = new GetProductByIdValidator();
            var req = new GetProductByIdRequest { ProductId = Guid.NewGuid() };
            var result = validator.Validate(req);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void GetProductByIdValidator_Should_Fail_On_Default_Guid()
        {
            var validator = new GetProductByIdValidator();
            var req = new GetProductByIdRequest { ProductId = default };
            var result = validator.Validate(req);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task CreateProductValidator_Should_Fail_When_ProductSku_Already_Exists()
        {
            var repo = Substitute.For<IProductRepository>();
            repo.ExistsAsync(
                Arg.Any<System.Linq.Expressions.Expression<System.Func<Catalog.Domain.Entities.ProductAggregate.Product, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<System.Threading.CancellationToken>())
                .Returns(true);
            var validator = Factory.CreateValidator<CreateProductValidator>(s => s.AddSingleton(repo));
            var req = new CreateProductRequest { ProductSku = "DUPLICATE-SKU" };
            var result = await validator.ValidateAsync(req, TestContext.Current.CancellationToken);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already Exists"));
        }
    }
}
