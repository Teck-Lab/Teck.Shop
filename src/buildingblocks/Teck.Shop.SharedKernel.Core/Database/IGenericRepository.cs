using System.Linq.Expressions;

namespace Teck.Shop.SharedKernel.Core.Database
{
    /// <summary>
    /// Generic repository interface combing both Read and Write repository interface.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface IGenericRepository<TEntity, in TId> : IGenericReadRepository<TEntity, TId>, IGenericWriteRepository<TEntity, TId>
        where TEntity : class
    {
    }

    /// <summary>
    /// Generic read repository.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface IGenericReadRepository<TEntity, in TId>
        where TEntity : class
    {
        /// <summary>
        /// Find by id and return found entity, or null.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enableTracking"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TEntity?> FindByIdAsync(TId id, bool enableTracking = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find one entity using predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="enableTracking"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, bool enableTracking = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find all entities matching the predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="enableTracking"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, bool enableTracking = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <param name="enableTracking"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IReadOnlyList<TEntity>> GetAllAsync(bool enableTracking = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if entity exist by predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="enableTracking"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, bool enableTracking = true, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Generic write repository.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface IGenericWriteRepository<TEntity, in TId>
        where TEntity : class
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="entity"></param>
        void Update(TEntity entity);

        /// <summary>
        /// Delete list of entities.
        /// </summary>
        /// <param name="entities"></param>
        void DeleteRange(IReadOnlyList<TEntity> entities);

        /// <summary>
        /// Delete a specific entity.
        /// </summary>
        /// <param name="entity"></param>
        void Delete(TEntity entity);

        /// <summary>
        /// Soft Delete list of entities without using the change tracker.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExcecutSoftDeleteAsync(IReadOnlyCollection<TId> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft delete entities matching the predicate, without using the change tracker.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExcecutSoftDeleteByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete entities matching the predicate, without using the change tracker.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task ExcecutHardDeleteAsync(IReadOnlyCollection<TId> ids, CancellationToken cancellationToken = default);
    }
}
