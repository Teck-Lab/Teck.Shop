using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore
{
    /// <summary>
    /// Defines a contract for dispatching integration events using a given DbContext and publish endpoint.
    /// </summary>
    public interface IIntegrationEventDispatcher
    {
        /// <summary>
        /// Dispatches integration events associated with the specified DbContext using the provided publish endpoint.
        /// </summary>
        /// <param name="context">The DbContext containing the events to dispatch.</param>
        /// <param name="publishEndpoint">The endpoint to publish events to.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DispatchIntegrationEventsAsync(DbContext context, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken = default);
    }
}