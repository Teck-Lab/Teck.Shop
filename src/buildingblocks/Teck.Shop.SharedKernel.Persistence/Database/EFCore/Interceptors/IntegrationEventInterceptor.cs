using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Teck.Shop.SharedKernel.Core.Domain;
using Teck.Shop.SharedKernel.Core.Events;

namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore.Interceptors
{
    /// <summary>
    /// The integration event interceptor.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="IntegrationEventInterceptor"/> class.
    /// </remarks>
    public sealed class IntegrationEventInterceptor : SaveChangesInterceptor
    {

        /// <summary>
        /// The publisher.
        /// </summary>
        private readonly IPublishEndpoint _publisher;


        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationEventInterceptor"/> class.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        public IntegrationEventInterceptor(IPublishEndpoint publisher)
        {
            _publisher = publisher;
        }
        /// <summary>
        /// Saved the changes asynchronously.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="result">The result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><![CDATA[ValueTask<int>]]></returns>
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                await PublishIntegrationEventsAsync(eventData.Context);
            }

            return result;
        }

        /// <summary>
        /// Publish integration events asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A Task.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HLQ004:The enumerator returns a reference to the item", Justification = "Add ref when .NET 9 comes out with support for it being async.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HLQ012:Consider using CollectionsMarshal.AsSpan()", Justification = "Add ref when .NET 9 comes out with support for it being async.")]
        private async Task PublishIntegrationEventsAsync(DbContext context)
        {
            List<IIntegrationEvent> integrationEvents = context
                .ChangeTracker
                .Entries<BaseEntity>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    IReadOnlyList<IIntegrationEvent> integrationEvents = entity.GetIntegrationEvents();
                    entity.ClearIntegrationEvents();
                    return integrationEvents;
                }).ToList();

            // Iterate over the list instead of using Span
            foreach (IIntegrationEvent integrationEvent in integrationEvents)
            {
                await _publisher.Publish(integrationEvent);
            }
        }
    }
}
