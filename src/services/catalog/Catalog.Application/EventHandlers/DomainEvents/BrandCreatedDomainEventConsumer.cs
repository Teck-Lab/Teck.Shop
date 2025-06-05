using Catalog.Domain.Entities.BrandAggregate.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Teck.Shop.SharedKernel.Events;

namespace Catalog.Application.EventHandlers.DomainEvents
{
    /// <summary>
    /// The brand created domain event consumer.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BrandCreatedDomainEventConsumer"/> class.
    /// </remarks>
    /// <param name="logger">The logger.</param>
    /// <param name="publishEndpoint"></param>
    public class BrandCreatedDomainEventConsumer(ILogger<BrandCreatedDomainEventConsumer> logger, IPublishEndpoint publishEndpoint) : IConsumer<BrandCreatedDomainEvent>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<BrandCreatedDomainEventConsumer> _logger = logger;

        /// <summary>
        /// Publish endpoint.
        /// </summary>
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        /// <summary>
        /// Consume the domain event.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "AV1755:Name of async method should end with Async or TaskAsync", Justification = "Masstransit consumer name should not contain async: https://masstransit.io/documentation/concepts/consumers")]
        public async Task Consume(ConsumeContext<BrandCreatedDomainEvent> context)
        {
            _logger.LogInformation($"Message is {{message}}", context.Message);

            BrandCreatedIntegrationEvent @event = new(context.Message.BrandId);
            await _publishEndpoint.Publish(@event, cancellationToken: context.CancellationToken);
        }
    }
}
