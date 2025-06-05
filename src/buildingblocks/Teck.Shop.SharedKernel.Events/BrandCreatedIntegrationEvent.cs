using Teck.Shop.SharedKernel.Core.Events;

namespace Teck.Shop.SharedKernel.Events
{
    /// <summary>
    /// The brand created integration event.
    /// </summary>
    public class BrandCreatedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// Gets the brand id.
        /// </summary>
        public Guid BrandId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrandCreatedIntegrationEvent"/> class.
        /// </summary>
        /// <param name="brandId">The brand id.</param>
        public BrandCreatedIntegrationEvent(Guid brandId)
        {
            BrandId = brandId;
        }
    }

}
