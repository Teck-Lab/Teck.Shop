using Teck.Shop.SharedKernel.Core.Options;

namespace Teck.Shop.SharedKernel.Infrastructure.Caching
{
    /// <summary>
    /// The caching options.
    /// </summary>
    public class CachingOptions : IOptionsRoot
    {
        /// <summary>
        /// Gets or sets the redis URL.
        /// </summary>
        public string? RedisURL { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string? Password { get; set; }
    }
}
