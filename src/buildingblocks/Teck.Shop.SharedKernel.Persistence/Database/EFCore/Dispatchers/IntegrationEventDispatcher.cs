using MassTransit;
using Microsoft.EntityFrameworkCore;
using Teck.Shop.SharedKernel.Core.Domain;
using Teck.Shop.SharedKernel.Core.Events;

namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore.Dispatchers
{
    /// <summary>
    /// Dispatches integration events from the DbContext to the publish endpoint.
    /// </summary>
    public class IntegrationEventDispatcher : IIntegrationEventDispatcher
    {
        /// <summary>
        /// Dispatches all integration events from the tracked entities in the specified DbContext using the provided publish endpoint.
        /// </summary>
        /// <param name="context">The DbContext containing tracked entities.</param>
        /// <param name="publishEndpoint">The endpoint used to publish integration events.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HLQ004:The enumerator returns a reference to the item", Justification = "Add ref when .NET 9 comes out with support for it being async.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HLQ012:Consider using CollectionsMarshal.AsSpan()", Justification = "Add ref when .NET 9 comes out with support for it being async.")]
        public async Task DispatchIntegrationEventsAsync(DbContext context, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken = default)
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
                await publishEndpoint.Publish(integrationEvent);
            }
        }
    }
}