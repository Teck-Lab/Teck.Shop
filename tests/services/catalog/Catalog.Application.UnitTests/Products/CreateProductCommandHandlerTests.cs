using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Application.Features.Products.CreateProduct.V1;
using Catalog.Application.Features.Products.Response;
using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.CategoryAggregate;
using Catalog.Domain.Entities.ProductAggregate;
using Catalog.Domain.Entities.BrandAggregate.Errors;
using Catalog.Domain.Entities.CategoryAggregate.Errors;
using Catalog.Domain.Entities.ProductAggregate.Errors;
using ErrorOr;
using NSubstitute;
using Xunit;
using Teck.Shop.SharedKernel.Core.CQRS;
using Teck.Shop.SharedKernel.Core.Database;

namespace Catalog.Application.UnitTests.Products
{
    public class CreateProductCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_ReturnProductResponse_WhenProductIsCreated()
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var productRepository = Substitute.For<IProductRepository>();
            var brandRepository = Substitute.For<IBrandRepository>();
            var categoryRepository = Substitute.For<ICategoryRepository>();
            var handler = new CreateProductCommandHandler(unitOfWork, productRepository, brandRepository, categoryRepository);
            var brand = new Brand();
            var categories = new List<Category> { new Category() };
            var command = new CreateProductCommand("Test Product", "desc", "sku", "gtin", true, Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });
            brandRepository.FindByIdAsync(Arg.Any<Guid>(), true, Arg.Any<CancellationToken>()).Returns(brand);
            categoryRepository.FindAsync(Arg.Any<Expression<Func<Category, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(categories);
            productRepository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
            var handlerInterface = (ICommandHandler<CreateProductCommand, ErrorOr<ProductResponse>>)handler;
            var result = await handlerInterface.Handle(command, CancellationToken.None);
            Assert.False(result.IsError);
        }

        [Fact]
        public async Task Handle_Should_ReturnBrandNotFound_WhenBrandDoesNotExist()
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var productRepository = Substitute.For<IProductRepository>();
            var brandRepository = Substitute.For<IBrandRepository>();
            var categoryRepository = Substitute.For<ICategoryRepository>();
            var handler = new CreateProductCommandHandler(unitOfWork, productRepository, brandRepository, categoryRepository);
            var command = new CreateProductCommand("Test Product", "desc", "sku", "gtin", true, Guid.NewGuid(), new List<Guid>());
            brandRepository.FindByIdAsync(Arg.Any<Guid>(), true, Arg.Any<CancellationToken>()).Returns((Brand)null);
            var handlerInterface = (ICommandHandler<CreateProductCommand, ErrorOr<ProductResponse>>)handler;
            var result = await handlerInterface.Handle(command, CancellationToken.None);
            Assert.True(result.IsError);
            Assert.Equal(BrandErrors.NotFound, result.FirstError);
        }

        [Fact]
        public async Task Handle_Should_ReturnCategoryNotFound_WhenCategoriesNotFound()
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var productRepository = Substitute.For<IProductRepository>();
            var brandRepository = Substitute.For<IBrandRepository>();
            var categoryRepository = Substitute.For<ICategoryRepository>();
            var handler = new CreateProductCommandHandler(unitOfWork, productRepository, brandRepository, categoryRepository);
            var command = new CreateProductCommand("Test Product", "desc", "sku", "gtin", true, null, new List<Guid> { Guid.NewGuid() });
            categoryRepository.FindAsync(Arg.Any<Expression<Func<Category, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new List<Category>());
            var handlerInterface = (ICommandHandler<CreateProductCommand, ErrorOr<ProductResponse>>)handler;
            var result = await handlerInterface.Handle(command, CancellationToken.None);
            Assert.True(result.IsError);
            Assert.Equal(CategoryErrors.NotFound, result.FirstError);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotCreated_WhenSaveChangesReturnsZero()
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var productRepository = Substitute.For<IProductRepository>();
            var brandRepository = Substitute.For<IBrandRepository>();
            var categoryRepository = Substitute.For<ICategoryRepository>();
            var handler = new CreateProductCommandHandler(unitOfWork, productRepository, brandRepository, categoryRepository);
            var brand = new Brand();
            var categories = new List<Category> { new Category() };
            var command = new CreateProductCommand("Test Product", "desc", "sku", "gtin", true, Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });
            brandRepository.FindByIdAsync(Arg.Any<Guid>(), true, Arg.Any<CancellationToken>()).Returns(brand);
            categoryRepository.FindAsync(Arg.Any<Expression<Func<Category, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(categories);
            productRepository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(0);
            var handlerInterface = (ICommandHandler<CreateProductCommand, ErrorOr<ProductResponse>>)handler;
            var result = await handlerInterface.Handle(command, CancellationToken.None);
            Assert.True(result.IsError);
            Assert.Equal(ProductErrors.NotCreated, result.FirstError);
        }
    }
}
