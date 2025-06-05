using Catalog.Application.Contracts.Caching;
using Catalog.Application.Contracts.Repositories;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.Mappings;
using Catalog.Domain.Entities.BrandAggregate;
using ErrorOr;
using Teck.Shop.SharedKernel.Core.CQRS;
using Teck.Shop.SharedKernel.Core.Database;

namespace Catalog.Application.Features.Brands.CreateBrand.V1
{
    /// <summary>
    /// Create brand command.
    /// </summary>
    public sealed record CreateBrandCommand(string Name, string? Description, string? Website) : ICommand<ErrorOr<BrandResponse>>;

    /// <summary>
    /// Create Brand command handler.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CreateBrandCommandHandler"/> class.
    /// </remarks>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="brandCache">The brand cache.</param>
    /// <param name="brandRepository">The brand repository.</param>
    internal sealed class CreateBrandCommandHandler(IUnitOfWork unitOfWork, IBrandCache brandCache, IBrandRepository brandRepository) : ICommandHandler<CreateBrandCommand, ErrorOr<BrandResponse>>
    {
        /// <summary>
        /// The unit of work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        /// The brand repository.
        /// </summary>
        private readonly IBrandRepository _brandRepository = brandRepository;

        /// <summary>
        /// The brand cache.
        /// </summary>
        private readonly IBrandCache _brandCache = brandCache;

        /// <summary>
        /// Handle and return a task of type erroror.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<ErrorOr<BrandResponse>>]]></returns>
        public async ValueTask<ErrorOr<BrandResponse>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            ErrorOr<Brand> brandToAdd = Brand.Create(
                request.Name!, request.Description, request.Website);

            if (brandToAdd.IsError)
            {
                return brandToAdd.Errors;
            }

            await _brandRepository.AddAsync(brandToAdd.Value, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _brandCache.SetAsync(brandToAdd.Value.Id, brandToAdd.Value, cancellationToken);
            return BrandMapper.BrandToBrandResponse(brandToAdd.Value);
        }
    }
}
