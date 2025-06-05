using System.ComponentModel.DataAnnotations;
using Teck.Shop.SharedKernel.Core.Options;

namespace Catalog.Infrastructure.Options
{
    /// <summary>
    /// The minio options.
    /// </summary>
    public class MinioOptions : IOptionsRoot
    {
        /// <summary>
        /// Gets or sets the access key id.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string AccessKeyId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the secret access key.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string SecretAccessKey { get; set; } = null!;

        /// <summary>
        /// Gets or sets the aws region.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string AwsRegion { get; set; } = null!;

        /// <summary>
        /// Gets or sets the minio server url.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string MinioServerUrl { get; set; } = null!;
    }
}
