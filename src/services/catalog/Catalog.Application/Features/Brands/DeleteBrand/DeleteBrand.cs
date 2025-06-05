using Catalog.Application.Contracts.Caching;
using Catalog.Application.Contracts.Repositories;
using Catalog.Domain.Entities.BrandAggregate;
using Catalog.Domain.Entities.BrandAggregate.Errors;
using ErrorOr;
using Teck.Shop.SharedKernel.Core.CQRS;
using Teck.Shop.SharedKernel.Core.Database;

namespace Catalog.Application.Features.Brands.DeleteBrand
{
    /// <summary>
    /// Delete brand command.
    /// </summary>
    public sealed record DeleteBrandCommand(Guid Id) : ICommand<ErrorOr<Deleted>>;

    /// <summary>
    /// Delete Brand command handler.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DeleteBrandCommandHandler"/> class.
    /// </remarks>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="cache">The cache.</param>
    /// <param name="brandRepository">The brand repository.</param>
    internal sealed class DeleteBrandCommandHandler(IUnitOfWork unitOfWork, IBrandCache cache, IBrandRepository brandRepository) : ICommandHandler<DeleteBrandCommand, ErrorOr<Deleted>>
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
        /// <returns><![CDATA[Task<ErrorOr<Deleted>>]]></returns>
        public async ValueTask<ErrorOr<Deleted>> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            Brand? brandToDelete = await _brandRepository.FindOneAsync(brand => brand.Id.Equals(request.Id), true, cancellationToken);

            if (brandToDelete is null)
            {
                return BrandErrors.NotFound;
            }

            _brandRepository.Delete(brandToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _cache.RemoveAsync(request.Id, cancellationToken);

            return Result.Deleted;
        }
    }
}
