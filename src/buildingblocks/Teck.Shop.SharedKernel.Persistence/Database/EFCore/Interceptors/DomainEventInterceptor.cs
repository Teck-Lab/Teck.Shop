using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Teck.Shop.SharedKernel.Core.Domain;
using Teck.Shop.SharedKernel.Core.Events;

namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore.Interceptors
{
    /// <summary>
    /// The domain event interceptor.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DomainEventInterceptor"/> class.
    /// </remarks>
    /// <param name="publisher">The publisher.</param>
    public sealed class DomainEventInterceptor(IMediator publisher) : SaveChangesInterceptor
    {
        /// <summary>
        /// The publisher.
        /// </summary>
        private readonly IMediator _publisher = publisher;

        /// <summary>
        /// Saved the changes asynchronously.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="result">The result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[ValueTask<int>]]></returns>
        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                await PublishDomainEventsAsync(eventData.Context);
            }

            return result;
        }

        /// <summary>
        /// Publish domain events asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A Task.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HLQ004:The enumerator returns a reference to the item", Justification = "Add ref when .NET 9 comes out with support for it being async.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HLQ012:Consider using CollectionsMarshal.AsSpan()", Justification = "Add ref when .NET 9 comes out with support for it being async.")]
        private async Task PublishDomainEventsAsync(DbContext context)
        {
            List<IDomainEvent> domainEvents = context
                .ChangeTracker
                .Entries<BaseEntity>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    IReadOnlyList<IDomainEvent> domainEvents = entity.GetDomainEvents();
                    entity.ClearDomainEvents();
                    return domainEvents;
                }).ToList();

            foreach (IDomainEvent domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent);
            }
        }
    }
}
