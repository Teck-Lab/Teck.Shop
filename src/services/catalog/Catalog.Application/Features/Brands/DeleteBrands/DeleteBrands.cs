using Catalog.Application.Contracts.Caching;
using Catalog.Application.Contracts.Repositories;
using ErrorOr;
using Teck.Shop.SharedKernel.Core.CQRS;

namespace Catalog.Application.Features.Brands.DeleteBrands
{
    /// <summary>
    /// Delete brands command.
    /// </summary>
    public sealed record DeleteBrandsCommand(IReadOnlyCollection<Guid> BrandIds) : ICommand<ErrorOr<Deleted>>;

    /// <summary>
    /// Delete brands command handler.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DeleteBrandsCommandHandler"/> class.
    /// </remarks>
    /// <param name="cache">The cache.</param>
    /// <param name="brandRepository">The brand repository.</param>
    internal sealed class DeleteBrandsCommandHandler(IBrandCache cache, IBrandRepository brandRepository) : ICommandHandler<DeleteBrandsCommand, ErrorOr<Deleted>>
    {
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
        public async ValueTask<ErrorOr<Deleted>> Handle(DeleteBrandsCommand request, CancellationToken cancellationToken)
        {
            await _brandRepository.ExcecutSoftDeleteAsync(request.BrandIds, cancellationToken);
            foreach (Guid id in request.BrandIds)
            {
                await _cache.RemoveAsync(id, cancellationToken);
            }

            return Result.Deleted;
        }
    }
}
