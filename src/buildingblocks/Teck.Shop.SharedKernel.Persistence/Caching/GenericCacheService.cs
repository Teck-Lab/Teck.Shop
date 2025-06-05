using Teck.Shop.SharedKernel.Core.Caching;
using Teck.Shop.SharedKernel.Core.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Teck.Shop.SharedKernel.Persistence.Caching
{
    /// <summary>
    /// The generic cache service.
    /// </summary>
    /// <typeparam name="TEntity"/>
    /// <typeparam name="TId"/>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GenericCacheService{TEntity, TId}"/> class.
    /// </remarks>
    /// <param name="fusionCache">The fusion cache.</param>
    /// <param name="genericRepository">The generic repository.</param>
    public class GenericCacheService<TEntity, TId>(IFusionCache fusionCache, IGenericRepository<TEntity, TId> genericRepository) : IGenericCacheService<TEntity, TId>
        where TEntity : class
    {
        /// <summary>
        /// The fusion cache.
        /// </summary>
        private readonly IFusionCache _fusionCache = fusionCache;

        /// <summary>
        /// The repository.
        /// </summary>
        private readonly IGenericRepository<TEntity, TId> _repository = genericRepository;

        /// <summary>
        /// The cache key prefix.
        /// </summary>
        private readonly string _cacheKeyPrefix = typeof(TEntity).Name;

        /// <summary>
        /// Get or set by id asynchronously.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="enableTracking">If true, enable tracking.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<TEntity?>]]></returns>
        public async Task<TEntity?> GetOrSetByIdAsync(TId id, bool enableTracking = false, CancellationToken cancellationToken = default)
        {
            string key = GenerateCacheKey(id!.ToString()!);

            return await _fusionCache.GetOrSetAsync<TEntity?>(
                key,
                async (context, ct) =>
                {
                    TEntity? result = await _repository.FindByIdAsync(id, enableTracking: false, cancellationToken: cancellationToken);
                    if (result is null)
                    {
                        context.Options.Duration = TimeSpan.FromMinutes(5);
                    }

                    return result;
                },
                token: cancellationToken);
        }

        /// <summary>
        /// Get or set by id asynchronously.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task]]></returns>
        public async Task SetAsync(TId id, TEntity entity, CancellationToken cancellationToken = default)
        {
            string key = GenerateCacheKey(id!.ToString()!);

            await _fusionCache.SetAsync(key, entity, token: cancellationToken);
        }

        /// <summary>
        /// Expire by id asynchronously, might not be removed, depends on the failsafe mode.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><![CDATA[Task]]></returns>
        public async Task ExpireAsync(TId id, CancellationToken cancellationToken = default)
        {
            string key = GenerateCacheKey(id!.ToString()!);

            await _fusionCache.ExpireAsync(key, token: cancellationToken);
        }

        /// <summary>
        /// Remove by id asynchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RemoveAsync(TId id, CancellationToken cancellationToken = default)
        {
            string key = GenerateCacheKey(id!.ToString()!);

            await _fusionCache.RemoveAsync(key, token: cancellationToken);
        }

        /// <summary>
        /// Generate cache key.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A string.</returns>
        public string GenerateCacheKey(params string[] data)
        {
            List<string> list = [_cacheKeyPrefix];

            list.AddRange(data);

            string key = string.Join(":", list);
            return key;
        }
    }
}
