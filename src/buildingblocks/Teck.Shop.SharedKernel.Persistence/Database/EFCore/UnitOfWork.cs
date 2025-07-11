using System.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Core.Domain;
using Teck.Shop.SharedKernel.Core.Events;
using Teck.Shop.SharedKernel.Core.Exceptions;

namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore
{
    /// <summary>
    /// The unit of work.
    /// </summary>
    /// <typeparam name="TContext"/>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
    /// </remarks>
    public class UnitOfWork<TContext> : IUnitOfWork
        where TContext : BaseDbContext
    {
        private readonly TContext _context;
        private IDbContextTransaction? _transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public UnitOfWork(TContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Begins the transaction asynchronously.
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="InvalidTransactionException">.</exception>
        /// <returns><![CDATA[Task<IDbContextTransaction>]]></returns>
        public async Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
            {
                throw new InvalidTransactionException("A transaction has already been started.");
            }

            _transaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return _transaction.GetDbTransaction();
        }

        /// <summary>
        /// Commits the transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="InvalidTransactionException">.</exception>
        /// <returns>A Task.</returns>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
            {
                throw new InvalidTransactionException("A transaction has not been started.");
            }

            await _transaction.CommitAsync(cancellationToken);
        }

        /// <summary>
        /// Creates transaction savepoint asynchronously.
        /// </summary>
        /// <param name="savePoint"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task CreateTransactionSavepoint(string savePoint, CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
            {
                throw new InvalidTransactionException("A transaction has not been started.");
            }

            await _transaction.CreateSavepointAsync(savePoint, cancellationToken);
        }

        /// <summary>
        /// Rollbacks the transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="InvalidTransactionException">.</exception>
        /// <returns>A Task.</returns>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
            {
                throw new InvalidTransactionException("A transaction has not been started.");
            }

            await _context.Database.RollbackTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Rollbacks the transaction to the savepoint asynchronously.
        /// </summary>
        /// <param name="savePoint"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task RollbackTransactionToSavepointAsync(string savePoint, CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
            {
                throw new InvalidTransactionException("A transaction has not been started.");
            }

            await _transaction.RollbackToSavepointAsync(savePoint, cancellationToken);
        }

        /// <summary>
        /// Save the changes asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[Task<int>]]></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HLQ004:The enumerator returns a reference to the item", Justification = "Add ref when .NET 9 comes out with support for it being async.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HLQ012:Consider using CollectionsMarshal.AsSpan()", Justification = "Add ref when .NET 9 comes out with support for it being async.")]
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await _context.SaveChangesAsync(cancellationToken);


            return result;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            _transaction?.Dispose();

            _context.Dispose();
        }
    }
}
