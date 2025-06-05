using MassTransit;
using Microsoft.Extensions.Logging;
using Teck.Shop.SharedKernel.Events;

namespace Catalog.Application.EventHandlers.IntegrationEvents
{
    /// <summary>
    /// The brand created domain event consumer.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BrandCreatedIntegrationEventConsumer"/> class.
    /// </remarks>
    /// <param name="logger">The logger.</param>
    public class BrandCreatedIntegrationEventConsumer(ILogger<BrandCreatedIntegrationEventConsumer> logger) : IConsumer<BrandCreatedIntegrationEvent>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<BrandCreatedIntegrationEventConsumer> _logger = logger;

        /// <summary>
        /// Consume the integration event.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Consume(ConsumeContext<BrandCreatedIntegrationEvent> context)
        {
            _logger.LogInformation($"Message is {{message}}", context.Message);
            return Task.CompletedTask;
        }
    }
}
