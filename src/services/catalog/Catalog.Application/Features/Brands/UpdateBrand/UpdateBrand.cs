using Catalog.Application.Contracts.Caching;
using Catalog.Application.Contracts.Repositories;
using Catalog.Application.Features.Brands.Dtos;
using Catalog.Application.Features.Brands.Mappings;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.BrandAggregate.Errors;
using ErrorOr;
using Mediator;
using Teck.Shop.SharedKernel.Core.Database;

namespace Catalog.Application.Features.Brands.UpdateBrand
{
    /// <summary>
    /// Update brand command.
    /// </summary>
    public sealed record UpdateBrandCommand(Guid Id, string? Name, string? Description, string? Website) : IRequest<ErrorOr<BrandResponse>>;

    /// <summary>
    /// The handler.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UpdateBrandCommandHandler"/> class.
    /// </remarks>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="cache">The cache.</param>
    /// <param name="brandRepository">The brand repository.</param>
    internal sealed class UpdateBrandCommandHandler(IUnitOfWork unitOfWork, IBrandCache cache, IBrandRepository brandRepository) : IRequestHandler<UpdateBrandCommand, ErrorOr<BrandResponse>>
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
        /// The cache.
        /// </summary>
        private readonly IBrandCache _cache = cache;

        /// <summary>
        /// Handle and return a task of type erroror.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<ErrorOr<BrandResponse>>]]></returns>

        public async ValueTask<ErrorOr<BrandResponse>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
        {
            Brand? brandToBeUpdated = await _brandRepository.FindByIdAsync(request.Id, cancellationToken: cancellationToken);

            if (brandToBeUpdated == null)
            {
                return BrandErrors.NotFound;
            }

            var updateOutcome = brandToBeUpdated.Update(
                request.Name, request.Description, request.Website);

            if (updateOutcome.IsError)
            {
                return updateOutcome.Errors;
            }

            _brandRepository.Update(brandToBeUpdated);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _cache.SetAsync(brandToBeUpdated.Id, brandToBeUpdated, cancellationToken: cancellationToken);

            return BrandMapper.BrandToBrandResponse(brandToBeUpdated);
        }
    }
}
