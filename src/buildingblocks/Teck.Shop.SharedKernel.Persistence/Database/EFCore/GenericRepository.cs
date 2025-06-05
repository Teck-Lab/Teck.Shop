using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Core.Domain;

namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore
{
    /// <summary>
    /// The generic repository.
    /// </summary>
    /// <typeparam name="TEntity"/>
    /// <typeparam name="TId"/>
    public class GenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId>
        where TEntity : class, IBaseEntity<TId>, IAggregateRoot
    {
        /// <summary>
        /// The db set.
        /// </summary>
        private readonly DbSet<TEntity> _dbSet;

        /// <summary>
        /// The http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor? _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{TEntity, TId}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="httpContextAccessor">The http context accessor.</param>
        public GenericRepository(BaseDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            BaseDbContext _context = context;
            _dbSet = _context.Set<TEntity>();
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Add entity async.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task.</returns>
        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="entity"></param>
        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Deletes the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void DeleteRange(IReadOnlyList<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// Exists and return a task of type bool asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="enableTracking">If true, enable tracking.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<bool>]]></returns>
        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            return enableTracking
                ? _dbSet.Where(predicate).AsTracking().AnyAsync(cancellationToken)
                : _dbSet.Where(predicate).AsNoTracking().AnyAsync(cancellationToken);
        }

        /// <summary>
        /// Finds and return a task of a read only list of tentities asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="enableTracking">If true, enable tracking.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<IReadOnlyList<TEntity>>]]></returns>
        public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            return enableTracking
                ? await _dbSet.Where(predicate).AsTracking().ToListAsync(cancellationToken)
                : await _dbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Find by id asynchronously.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="enableTracking">If true, enable tracking.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<TEntity?>]]></returns>
        public async Task<TEntity?> FindByIdAsync(TId id, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            return await FindOneAsync(entity => entity.Id!.Equals(id), enableTracking, cancellationToken);
        }

        /// <summary>
        /// Finds the one asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="enableTracking">If true, enable tracking.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<TEntity?>]]></returns>
        public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            TEntity? result = enableTracking
                ? await _dbSet.Where(predicate).AsTracking().SingleOrDefaultAsync(cancellationToken)
                : await _dbSet.Where(predicate).AsNoTracking().SingleOrDefaultAsync(cancellationToken);

            return result;
        }

        /// <summary>
        /// Get the all asynchronously.
        /// </summary>
        /// <param name="enableTracking">If true, enable tracking.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<IReadOnlyList<TEntity>>]]></returns>
        public async Task<IReadOnlyList<TEntity>> GetAllAsync(bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            if (enableTracking)
            {
                await _dbSet.AsQueryable().AsTracking().ToListAsync(cancellationToken);
            }

            return await _dbSet.AsQueryable().AsNoTracking().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Excecuts soft delete asynchronously.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public async Task ExcecutSoftDeleteAsync(IReadOnlyCollection<TId> ids, CancellationToken cancellationToken = default)
        {
            string? currentUserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _dbSet.Where(entity => ids.Contains(entity.Id)).ExecuteUpdateAsync(setters => setters.SetProperty(entity => entity.IsDeleted, true).SetProperty(entity => entity.DeletedOn, DateTimeOffset.UtcNow).SetProperty(entity => entity.DeletedBy, currentUserId), cancellationToken);
        }

        /// <summary>
        /// Excecuts soft delete by asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public async Task ExcecutSoftDeleteByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            string? currentUserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _dbSet.Where(predicate).ExecuteUpdateAsync(setters => setters.SetProperty(entity => entity.IsDeleted, true).SetProperty(entity => entity.DeletedOn, DateTimeOffset.UtcNow).SetProperty(entity => entity.DeletedBy, currentUserId), cancellationToken);
        }

        /// <summary>
        /// Excecuts hard delete asynchronously.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public async Task ExcecutHardDeleteAsync(IReadOnlyCollection<TId> ids, CancellationToken cancellationToken = default)
        {
            await _dbSet.Where(entity => ids.Contains(entity.Id)).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
